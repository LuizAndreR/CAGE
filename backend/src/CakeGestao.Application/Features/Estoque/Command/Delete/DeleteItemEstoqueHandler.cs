using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Estoque.Command.Delete;

public class DeleteItemEstoqueHandler : IRequestHandler<DeleteItemEstoqueCommand, Result>
{
    private readonly IEstoqueRepository _estoqueRepository;
    private readonly IValidator<DeleteItemEstoqueCommand> _validator;
    private readonly ILogger<DeleteItemEstoqueHandler> _logger;
    private const string UseCaseLogPrefix = "[Delete Item Estoque]";

    public DeleteItemEstoqueHandler(IEstoqueRepository estoqueRepository, IValidator<DeleteItemEstoqueCommand> validator, ILogger<DeleteItemEstoqueHandler> logger)
    {
        _estoqueRepository = estoqueRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteItemEstoqueCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Iniciando execução do caso de uso.", UseCaseLogPrefix);

        _logger.LogInformation("{LogPrefix} Validando requisição...", UseCaseLogPrefix);
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            _logger.LogInformation("{LogPrefix} Validação falhou: {Errors}", UseCaseLogPrefix, validationResult.Errors);
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result.Fail(new ValidationError(errors));
        }
        _logger.LogInformation("{LogPrefix} Requisição validada com sucesso.", UseCaseLogPrefix);

        _logger.LogInformation("{LogPrefix} Verificando existência do item de estoque a ser deletado...", UseCaseLogPrefix);
        var itemEstoqueResult = await _estoqueRepository.GetItemEstoqueByIdAsync(request.ItemId);
        if (itemEstoqueResult.IsFailed)
        {
            _logger.LogInformation("{LogPrefix} Item de estoque não encontrado para exclusão. ItemId: {ItemId}", UseCaseLogPrefix, request.ItemId);
            return Result.Fail(new NotFoundError("Item de estoque não encontrado para exclusão."));
        }
        var itemEstoque = itemEstoqueResult.Value;
        _logger.LogInformation("{LogPrefix} Item de estoque encontrado para exclusão. ItemId: {ItemId}", UseCaseLogPrefix, request.ItemId);

        _logger.LogInformation("{LogPrefix} Deletando item de estoque no repositório...", UseCaseLogPrefix);
        await _estoqueRepository.DeleteItemEstoqueAsync(itemEstoque);
        _logger.LogInformation("{LogPrefix} Item de estoque deletado com sucesso.", UseCaseLogPrefix);

        _logger.LogInformation("{LogPrefix} Execução do caso de uso concluída com sucesso.", UseCaseLogPrefix);
        return Result.Ok();
    }
}
