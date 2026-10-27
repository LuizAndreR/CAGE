using ItemEstoque = CakeGestao.Domain.Entities.Estoque;
using CakeGestao.Application.Common;
using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Exceptions;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Pedidos.Command.UpdateStatus;

public class UpdateStatusPedidoHandler : IRequestHandler<UpdateStatusPedidoCommand, Result>
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly ILogger<UpdateStatusPedidoHandler> _logger;
    private readonly IValidator<UpdateStatusPedidoCommand> _validator;
    private readonly IReceitaRepository _receitaRepository;
    private readonly IEstoqueRepository _estoqueRepository;
    private const string LogPrefix = "[Update Status Pedido Handler]";

    public UpdateStatusPedidoHandler(IPedidoRepository pedidoRepository, ILogger<UpdateStatusPedidoHandler> logger, IValidator<UpdateStatusPedidoCommand> validator, IReceitaRepository receitaRepository, IEstoqueRepository estoqueRepository)
    {
        _pedidoRepository = pedidoRepository;
        _logger = logger;
        _validator = validator;
        _receitaRepository = receitaRepository;
        _estoqueRepository = estoqueRepository;
    }

    public async Task<Result> Handle(UpdateStatusPedidoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Iniciando atualização do status do pedido {PedidoId} da empresa {EmpresaId}", LogPrefix, request.PedidoId, request.EmpresaId);
        
        var validatorResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validatorResult.IsValid)
        {
            var errors = validatorResult.Errors.Select(x => x.ErrorMessage).ToList();
            _logger.LogWarning("{LogPrefix} Validação falhou. Pedido: {PedidoId}. Erros: {Errors}", LogPrefix, request.PedidoId, string.Join(", ", errors));
            return Result.Fail(new ValidationError(errors)); 
        }
        
        var pedidoResult = await _pedidoRepository.GetByIdAsync(request.PedidoId, request.EmpresaId);
        if (pedidoResult.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Pedido {PedidoId} não encontrado.", LogPrefix, request.PedidoId);
            return Result.Fail(new NotFoundError("Pedido não encontrado."));
        }
        Pedido pedido = pedidoResult.Value;
        StatusPedidoEnum statusPedido = Enum.Parse<StatusPedidoEnum>(request.Status, ignoreCase: true);

        var emProducao = pedido.StatusPedido == StatusPedidoEnum.Pendente && statusPedido == StatusPedidoEnum.EmProducao;
        var devolvendo = pedido.StatusPedido == StatusPedidoEnum.EmProducao && statusPedido == StatusPedidoEnum.Pendente;
        var avisos = new List<EstoqueNegativoAviso>();

        if (emProducao || devolvendo)
        {
            var planoResult = await PlanejarAsync(pedido, request.EmpresaId, devolvendo, cancellationToken);
            if (planoResult.IsFailed) return Result.Fail(planoResult.Errors);

            // Valide todos os ingredientes antes de alterar os saldos.
            // Os repositórios compartilham o contexto: pedido, consumo e estoque são salvos juntos abaixo.
            if (emProducao) pedido.ConsumosEstoque.Clear();
            foreach (var (item, quantidade) in planoResult.Value.Values)
            {
                if (devolvendo) item.DevolverQuantidade(quantidade);
                else if (quantidade > 0)
                {
                    item.RemoverQuantidade(quantidade);
                    pedido.ConsumosEstoque.Add(new(item.Id, quantidade, item.UnidadeMedida));
                    if (item.QuantidadeAtual < 0)
                        avisos.Add(new(item));
                }
            }
            if (devolvendo) pedido.ConsumosEstoque.Clear();
        }

        pedido.AlterarStatus(statusPedido);
        var salvo = await _pedidoRepository.UpdatePedidoAsync(pedido);
        if (salvo.IsFailed) return salvo;
        var resultado = Result.Ok();
        foreach (var aviso in avisos) resultado.WithSuccess(aviso);
        return resultado;
    }

    private async Task<Result<Dictionary<int, (ItemEstoque Item, decimal Quantidade)>>> PlanejarAsync(
        Pedido pedido, int empresaId, bool devolver, CancellationToken cancellationToken)
    {
        var plano = new Dictionary<int, (ItemEstoque Item, decimal Quantidade)>();
        async Task<Result<ItemEstoque>> Buscar(int id)
        {
            if (plano.TryGetValue(id, out var carregado)) return Result.Ok(carregado.Item);
            var result = await _estoqueRepository.GetItemEstoqueByIdAsync(id, empresaId);
            if (result.IsFailed) return Result.Fail(new NotFoundError($"Item de estoque {id} não encontrado."));
            plano[id] = (result.Value, 0);
            return result;
        }
        void Somar(ItemEstoque item, decimal quantidade) =>
            plano[item.Id] = (item, plano[item.Id].Quantidade + quantidade);

        // Pedidos produzidos após esta alteração devolvem o consumo real, sem consultar a receita atual.
        if (devolver && pedido.ConsumosEstoque.Count > 0)
        {
            foreach (var consumo in pedido.ConsumosEstoque)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var result = await Buscar(consumo.ItemId);
                if (result.IsFailed) return Result.Fail(result.Errors);
                var item = result.Value;
                var quantidade = ConversorUnidade.Converter(consumo.Quantidade, consumo.UnidadeMedida,
                    item.UnidadeMedida, item.UnidadeReferenciaVolume, item.PesoReferenciaEmGramas);
                if (quantidade.IsFailed) return Result.Fail(new ValidationError(quantidade.Errors[0].Message));
                Somar(item, quantidade.Value);
            }
        }
        else
        {
            foreach (var itemPedido in pedido.Itens)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var receitaResult = await _receitaRepository.GetReceitaByIdAsync(itemPedido.ReceitaId, empresaId);
                if (receitaResult.IsFailed)
                    return Result.Fail(new NotFoundError($"Receita {itemPedido.ReceitaId} não encontrada."));

                foreach (var ingrediente in receitaResult.Value.Ingredientes)
                {
                    var principalResult = await Buscar(ingrediente.ItemId);
                    if (principalResult.IsFailed) return Result.Fail(principalResult.Errors);
                    var principal = principalResult.Value;
                    var restante = ingrediente.Quantidade * itemPedido.Quantidade;
                    var fatorPrincipal = ConversorUnidade.Converter(1, ingrediente.UnidadeMedida,
                        principal.UnidadeMedida, principal.UnidadeReferenciaVolume, principal.PesoReferenciaEmGramas);
                    if (fatorPrincipal.IsFailed) return Result.Fail(new ValidationError(fatorPrincipal.Errors[0].Message));

                    // Compatibilidade com pedidos que já estavam em produção antes do registro de consumo.
                    if (devolver)
                    {
                        Somar(principal, restante * fatorPrincipal.Value);
                        continue;
                    }

                    var candidatos = new[] { ingrediente.ItemId }.Concat(
                        ingrediente.Alternativas.OrderBy(a => a.Ordem).Select(a => a.ItemId));
                    foreach (var id in candidatos)
                    {
                        if (restante <= 0) break;
                        var result = await Buscar(id);
                        if (result.IsFailed) return Result.Fail(result.Errors);
                        var item = result.Value;
                        if (!AlternativasIngrediente.UnidadesCompativeis(principal.UnidadeMedida, item.UnidadeMedida))
                            return Result.Fail(new ValidationError($"A alternativa {item.Nome} — {item.Marca} possui uma unidade incompatível com o principal. Revise a receita."));
                        var fator = ConversorUnidade.Converter(1, ingrediente.UnidadeMedida,
                            item.UnidadeMedida, item.UnidadeReferenciaVolume, item.PesoReferenciaEmGramas);
                        if (fator.IsFailed) return Result.Fail(new ValidationError(fator.Errors[0].Message));
                        var disponivel = Math.Max(0, item.QuantidadeAtual - plano[id].Quantidade);
                        var usado = Math.Min(restante, disponivel / fator.Value);
                        Somar(item, usado * fator.Value);
                        restante = Math.Max(0, restante - usado);
                    }
                    if (restante > 0) Somar(principal, restante * fatorPrincipal.Value);
                }
            }
        }
        // Mesma precisão das colunas de estoque e do registro usado na devolução.
        foreach (var id in plano.Keys.ToList())
            plano[id] = (plano[id].Item, Math.Round(plano[id].Quantidade, 6, MidpointRounding.AwayFromZero));
        return Result.Ok(plano);
    }
}
