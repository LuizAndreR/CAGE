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
        
        var validatorResult = await _validator.ValidateAsync(request);
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

        if (pedido.StatusPedido == StatusPedidoEnum.Pendente && statusPedido == StatusPedidoEnum.EmProducao)
        {
            _logger.LogInformation("{LogPrefix} Pedido {Id} entrou em produção. Iniciando baixa de estoque...", LogPrefix, pedido.Id);

            foreach (ItemPedido itemPedido in pedido.Itens)
            {
                var receitaResult = await _receitaRepository.GetReceitaByIdAsync(itemPedido.ReceitaId, request.EmpresaId);
                
                if (receitaResult.IsFailed)continue;
                Receita receita = receitaResult.Value;

                foreach (var ingrediente in receita.Ingredientes)
                {
                    var resultItemEstoque = await _estoqueRepository.GetItemEstoqueByIdAsync(ingrediente.ItemId, request.EmpresaId);
                    
                    if (resultItemEstoque.IsSuccess)
                    {
                        var itemEstoque = resultItemEstoque.Value;
                        decimal quantidadeParaAbater = ingrediente.Quantidade * itemPedido.Quantidade;

                        itemEstoque.RemoverQuantidade(quantidadeParaAbater);

                        await _estoqueRepository.UpdateItemEstoqueAsync(itemEstoque);

                        _logger.LogInformation("{LogPrefix} Baixa de Estoque: {Nome} - Abatido: {Qtd} {Unidade}", LogPrefix, itemEstoque.Nome, quantidadeParaAbater, itemEstoque.UnidadeMedida);
                    }
                }
            }
        }
        
        if (pedido.StatusPedido == StatusPedidoEnum.EmProducao && statusPedido == StatusPedidoEnum.Pendente)
        {
            var quantidades = new Dictionary<int, decimal>();
            foreach (var itemPedido in pedido.Itens)
            {
                var receitaResult = await _receitaRepository.GetReceitaByIdAsync(itemPedido.ReceitaId, request.EmpresaId);
                if (receitaResult.IsFailed)
                    return Result.Fail(new NotFoundError($"Receita {itemPedido.ReceitaId} não encontrada para devolver os ingredientes."));

                foreach (var ingrediente in receitaResult.Value.Ingredientes)
                    quantidades[ingrediente.ItemId] = quantidades.GetValueOrDefault(ingrediente.ItemId)
                        + ingrediente.Quantidade * itemPedido.Quantidade;
            }

            // Confira todos os itens antes de iniciar a devolução.
            var devolucoes = new List<(CakeGestao.Domain.Entities.Estoque Item, decimal Quantidade)>();
            foreach (var quantidade in quantidades)
            {
                var estoqueResult = await _estoqueRepository.GetItemEstoqueByIdAsync(quantidade.Key, request.EmpresaId);
                if (estoqueResult.IsFailed)
                    return Result.Fail(new NotFoundError($"Item de estoque {quantidade.Key} não encontrado para devolução."));
                devolucoes.Add((estoqueResult.Value, quantidade.Value));
            }

            foreach (var devolucao in devolucoes)
            {
                devolucao.Item.DevolverQuantidade(devolucao.Quantidade);
                await _estoqueRepository.UpdateItemEstoqueAsync(devolucao.Item);
                _logger.LogInformation("{LogPrefix} Devolução de estoque do pedido {PedidoId}: {Quantidade} {Unidade} de {Nome}.",
                    LogPrefix, pedido.Id, devolucao.Quantidade, devolucao.Item.UnidadeMedida, devolucao.Item.Nome);
            }
        }

        pedido.AlterarStatus(statusPedido);
        
        await _pedidoRepository.UpdatePedidoAsync(pedido);  
        
        return Result.Ok();
    }
}