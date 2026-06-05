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
    private const string LogPrefix = "[Update Status Pedido Handler]";

    public UpdateStatusPedidoHandler(IPedidoRepository pedidoRepository, ILogger<UpdateStatusPedidoHandler> logger, IValidator<UpdateStatusPedidoCommand> validator)
    {
        _pedidoRepository = pedidoRepository;
        _logger = logger;
        _validator = validator;
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
        
        StatusPedidoEnum statusPedido = Enum.Parse<StatusPedidoEnum>(request.Status);
        pedido.AlterarStatus(statusPedido);
        
        await _pedidoRepository.UpdatePedidoAsync(pedido);
        
        return Result.Ok();
    }
}