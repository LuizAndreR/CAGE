using AutoMapper;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Estoque.Command.RemoverQuantidade;

public class RemoverQuantidadeEstoqueHandler : IRequestHandler<RemoveQuantidadeEstoqueCommand, Result>
{
    private readonly IEstoqueRepository _estoqueRepository;
    private readonly ILogger<RemoverQuantidadeEstoqueHandler> _logger;
    private readonly IValidator<RemoveQuantidadeEstoqueCommand> _validator;
    private const string LogPrefix = "[Remove Quantidade Estoque Handler]";

    public RemoverQuantidadeEstoqueHandler(IEstoqueRepository estoqueRepository, ILogger<RemoverQuantidadeEstoqueHandler> logger, IValidator<RemoveQuantidadeEstoqueCommand> validator)
    {
        _estoqueRepository = estoqueRepository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result> Handle(RemoveQuantidadeEstoqueCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Iniciando baixa manual de estoque. ItemID: {ItemId} | Qtd a remover: {Qtd}", LogPrefix, request.ItemId, request.QuantidadeARemover);

        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("{LogPrefix} Dados inválidos. ItemID: {ItemId}. Erros: {Errors}", LogPrefix, request.ItemId, string.Join(", ", errors));
            return Result.Fail(new ValidationError(errors));
        }

        var itemEstoqueResult = await _estoqueRepository.GetItemEstoqueByIdAsync(request.ItemId, request.EmpresaId);
        if (itemEstoqueResult.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Item não encontrado. ItemID: {ItemId}", LogPrefix, request.ItemId);
            return Result.Fail(new NotFoundError("Item de estoque não encontrado para remoção de quantidade."));
        }
        var itemEstoque = itemEstoqueResult.Value;

        itemEstoque.RemoverQuantidade(request.QuantidadeARemover);

        await _estoqueRepository.UpdateItemEstoqueAsync(itemEstoque);

        _logger.LogInformation("{LogPrefix} Baixa realizada com sucesso. ID: {ItemId} | Nova Qtd: {Qtd}", LogPrefix, itemEstoque.Id, itemEstoque.QuantidadeAtual);
        return Result.Ok();
    }
}
