using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Exceptions;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Pedidos.Command.Delete;

public class DeletePedidoHandler : IRequestHandler<DeletePedidoCommand, Result>
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IValidator<DeletePedidoCommand> _deletePedidoValidator;
    private readonly ILogger<DeletePedidoHandler> _logger;
    private const string LogPrefix = "[Delete Pagamento Pedido Handler]";

    public DeletePedidoHandler(IPedidoRepository pedidoRepository, IValidator<DeletePedidoCommand> deletePedidoValidator, ILogger<DeletePedidoHandler> logger)
    {
        _pedidoRepository = pedidoRepository;
        _deletePedidoValidator = deletePedidoValidator;
        _logger = logger;
    }

    public async Task<Result> Handle(DeletePedidoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Iniciando delete do pedido {PedidoId} da empresa {EmpresaId}", LogPrefix, request.PedidoId, request.EmpresaId);

        var validatorResult = await _deletePedidoValidator.ValidateAsync(request);
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
        
        await _pedidoRepository.DeletePedidoAsync(pedido);
        
        return Result.Ok();
    }
}