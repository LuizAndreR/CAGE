using CakeGestao.Application.Features.Financeiro.Command.Cancelamento;
using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Exceptions;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Pedidos.Command.Update;

public class UpdatePedidoHandler : IRequestHandler<UpdatePedidoCommand, Result>
{
    private readonly IValidator<UpdatePedidoCommand> _validator;
    private readonly ILogger<UpdatePedidoHandler> _logger;
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IReceitaRepository _receitaRepository;
    private readonly IFinanceiroRepository _financeiroRepository;
    private readonly IMediator _mediator;
    private const string LogPrefix = "[Update Pedido Handler]";
    
    public UpdatePedidoHandler(IValidator<UpdatePedidoCommand> validator, ILogger<UpdatePedidoHandler> logger, IPedidoRepository pedidoRepository, IReceitaRepository receitaRepository, IFinanceiroRepository financeiroRepository, IMediator mediator)
    {
        _validator = validator;
        _logger = logger;
        _pedidoRepository = pedidoRepository;
        _receitaRepository = receitaRepository;
        _financeiroRepository = financeiroRepository;
        _mediator = mediator;
    }

    public async Task<Result> Handle(UpdatePedidoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Iniciando atualização do pedido {PedidoId} da empresa {EmpresaId}", LogPrefix, request.Id, request.EmpresaId);
        
        var validatorResult = await _validator.ValidateAsync(request);
        if (!validatorResult.IsValid)
        {
            var errors = validatorResult.Errors.Select(x => x.ErrorMessage).ToList();
            _logger.LogWarning("{LogPrefix} Validação falhou. Pedido: {PedidoId}. Erros: {Errors}", LogPrefix, request.Id, string.Join(", ", errors));
            return Result.Fail(new ValidationError(errors));
        }      
        
        var pedidoResult = await _pedidoRepository.GetByIdAsync(request.Id, request.EmpresaId);
        if (pedidoResult.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Pedido {PedidoId} não encontrado.", LogPrefix, request.Id);
            return Result.Fail(new NotFoundError("Pedido não encontrado."));
        }
        Pedido pedido = pedidoResult.Value;
        bool itensMudados = VerificarSeItensMudaram(pedido.Itens, request.Itens);

        if (itensMudados)
        {
            var novosItens = new List<ItemPedido>();
            foreach (var itemDto in request.Itens)
            {
                var receitaResult = await _receitaRepository.GetReceitaByIdAsync(itemDto.ReceitaId, request.EmpresaId);
                if (receitaResult.IsFailed)
                    return Result.Fail(new NotFoundError($"A receita com ID {itemDto.ReceitaId} não foi encontrada."));

                var receita = receitaResult.Value;
                novosItens.Add(new ItemPedido(receita.Id, itemDto.Quantidade, receita.PrecoVenda));
            }

            if (pedido.Pago)
            {
                if (request.UsuarioId <= 0)
                    return Result.Fail(new ValidationError("Usuário responsável pela edição inválido."));

                var transacaoResult = await _financeiroRepository.GetTransacaoByPedidoId(pedido.Id, request.EmpresaId);
                if (transacaoResult.IsFailed)
                    return Result.Fail(new ConflictError("Não foi encontrado um pagamento ativo para cancelar. Verifique o financeiro do pedido antes de editar os itens."));

                var cancelResult = await _mediator.Send(new CancelTransacaoCommand
                {
                    TransacaoId = transacaoResult.Value.Id,
                    EmpresaId = request.EmpresaId,
                    UsuarioId = request.UsuarioId,
                    Motivo = $"Cancelamento automático por alteração dos itens ou quantidades do Pedido #{pedido.Id}."
                }, cancellationToken);

                if (cancelResult.IsFailed)
                    return Result.Fail(cancelResult.Errors);
            }

            pedido.LimparItens();
            foreach (var item in novosItens)
                pedido.AdicionarItem(item);
        }

        pedido.Atualizar(request.ClienteNome, request.TelefoneCliente, request.Descricao, request.DataEntrega);

        await _pedidoRepository.UpdatePedidoAsync(pedido); 

        return Result.Ok();
    }

    private bool VerificarSeItensMudaram(ICollection<ItemPedido> itensBanco,
        List<UpdatePedidoItemDto> itensRequest)
    {
        var atuais = itensBanco.Select(i => (i.ReceitaId, i.Quantidade))
            .OrderBy(i => i.ReceitaId).ThenBy(i => i.Quantidade);
        var novos = itensRequest.Select(i => (i.ReceitaId, i.Quantidade))
            .OrderBy(i => i.ReceitaId).ThenBy(i => i.Quantidade);
        return !atuais.SequenceEqual(novos);
    }
}
