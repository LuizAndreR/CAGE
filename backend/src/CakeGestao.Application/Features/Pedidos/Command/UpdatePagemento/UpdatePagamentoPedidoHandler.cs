using CakeGestao.Application.Features.Financeiro.Command.Cancelamento;
using CakeGestao.Application.Features.Financeiro.Command.Create;
using CakeGestao.Domain.Exceptions;
using CakeGestao.Domain.Interfaces.Repositories;
using CakeGestao.Domain.Entities;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Pedidos.Command.UpdatePagemento;

public class UpdatePagamentoPedidoHandler : IRequestHandler<UpdatePagamentoPedidoCommand, Result>
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IValidator<UpdatePagamentoPedidoCommand> _validator;
    private readonly IMediator _mediator;   
    private readonly IFinanceiroRepository _transacaoRepository;
    private readonly ILogger<UpdatePagamentoPedidoHandler> _logger;
    private const string LogPrefix = "[Update Pagamento Pedido Handler]";
    
    public UpdatePagamentoPedidoHandler(IPedidoRepository pedidoRepository, IValidator<UpdatePagamentoPedidoCommand> validator, IMediator mediator, ILogger<UpdatePagamentoPedidoHandler> logger, IFinanceiroRepository transacaoRepository)
    {
        _pedidoRepository = pedidoRepository;
        _validator = validator;
        _mediator = mediator;
        _logger = logger;
        _transacaoRepository = transacaoRepository;
    }

    public async Task<Result> Handle(UpdatePagamentoPedidoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Iniciando atualização do pagamento do pedido {PedidoId} da empresa {EmpresaId}", LogPrefix, request.PedidoId, request.EmpresaId);
        
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

        if (request.Pagamento && !pedido.Pago)
        {
            pedido.InformarPagamento();
           
            var transacaoCommand = new CreateTransacaoCommand
            {
                EmpresaId = pedido.EmpresaId,
                PedidoId = pedido.Id,
                Tipo = "Entrada",
                Categoria = "Vendas", 
                Valor = pedido.ValorTotal,
                Data = pedido.DataPagamento ?? DateTime.UtcNow,
                Descricao = $"Pagamento do Pedido #{pedido.Id} - {pedido.ClienteNome}"
            };

            var transacaoResult = await _mediator.Send(transacaoCommand, cancellationToken);

            if (transacaoResult.IsFailed)
            {
                _logger.LogWarning("[Pagamento] Falha ao gerar transação financeira para o pedido {Id}.", pedido.Id);
                return Result.Fail("Não foi possível gerar a transação financeira.").WithErrors(transacaoResult.Errors);
            }

            _logger.LogInformation("[Pagamento] Pedido {Id} pago. Transação gerada com sucesso.", pedido.Id);
        }
        else if (!request.Pagamento && pedido.Pago)
        {
            pedido.EstornarPagamento();

            var transacaoResult = await _transacaoRepository.GetTransacaoByPedidoId(pedido.Id, request.EmpresaId);
            TransacaoFinanceira transacao = transacaoResult.Value;
            
            if (transacaoResult.IsSuccess)
            {
                var cancelCommand = new CancelTransacaoCommand
                {
                    TransacaoId = transacao.Id,
                    EmpresaId = pedido.EmpresaId,
                    UsuarioId = request.UsuarioId,
                    Motivo = $"Estorno automático gerado pelo desfazimento do pagamento do Pedido #{pedido.Id} - Cliente: {pedido.ClienteNome}"
                };

                var cancelResult = await _mediator.Send(cancelCommand, cancellationToken);

                if (cancelResult.IsFailed)
                {
                    _logger.LogWarning("[Pagamento] Falha ao cancelar a transação financeira {TransacaoId} para o pedido {PedidoId}.", transacao.Id, pedido.Id);
                    return Result.Fail("Não foi possível estornar a transação financeira no caixa.").WithErrors(cancelResult.Errors);
                }

                _logger.LogInformation("[Pagamento] Pagamento do pedido {Id} estornado. Transação financeira cancelada com sucesso.", pedido.Id);
            }
        }
        await _pedidoRepository.UpdatePedidoAsync(pedido); 
        return Result.Ok();
    }
}