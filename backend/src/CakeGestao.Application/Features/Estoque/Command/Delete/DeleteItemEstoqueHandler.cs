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
    private const string LogPrefix = "[Delete Estoque Handler]";

    public DeleteItemEstoqueHandler(IEstoqueRepository estoqueRepository, IValidator<DeleteItemEstoqueCommand> validator, ILogger<DeleteItemEstoqueHandler> logger)
    {
        _estoqueRepository = estoqueRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteItemEstoqueCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Solicitada exclusão de item de estoque. ID: {ItemId} | EmpresaID: {EmpresaId}", LogPrefix, request.ItemId, request.EmpresaId);

        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("{LogPrefix} Requisição inválida. ID: {ItemId}. Erros: {Errors}", LogPrefix, request.ItemId, string.Join(", ", errors));
            return Result.Fail(new ValidationError(errors));
        }

        var itemEstoqueResult = await _estoqueRepository.GetItemEstoqueByIdAsync(request.ItemId, request.EmpresaId);
        if (itemEstoqueResult.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Item não encontrado para exclusão. ID: {ItemId}", LogPrefix, request.ItemId);
            return Result.Fail(new NotFoundError("Item de estoque não encontrado para exclusão."));
        }
        var itemEstoque = itemEstoqueResult.Value;

        await _estoqueRepository.DeleteItemEstoqueAsync(itemEstoque);

        _logger.LogInformation("{LogPrefix} Item excluído permanentemente. ID: {ItemId} | Nome: {Nome}", LogPrefix, request.ItemId, itemEstoque.Nome);
        return Result.Ok();
    }
}
