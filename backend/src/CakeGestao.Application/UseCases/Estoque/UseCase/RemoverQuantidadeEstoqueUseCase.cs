using AutoMapper;
using CakeGestao.Application.Dtos.Requests.Estoque;
using CakeGestao.Application.UseCases.Estoque.Interface;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.Estoque.UseCase;

public class RemoverQuantidadeEstoqueUseCase : IRemoverQuantidadeEstoqueUseCase
{
    private readonly IEstoqueRepository _estoqueRepository;
    private readonly ILogger<RemoverQuantidadeEstoqueUseCase> _logger;
    private readonly IValidator<RemoveQuantidadeEstoqueRequest> _validator;
    private readonly IMapper _mapper;
    private const string UseCaseLogPrefix = "[Remover Quantidade Estoque]";

    public RemoverQuantidadeEstoqueUseCase(IEstoqueRepository estoqueRepository, ILogger<RemoverQuantidadeEstoqueUseCase> logger, IValidator<RemoveQuantidadeEstoqueRequest> validator, IMapper mapper)
    {
        _estoqueRepository = estoqueRepository;
        _logger = logger;
        _validator = validator;
        _mapper = mapper;
    }

    public async Task<Result> ExecuteAsync(RemoveQuantidadeEstoqueRequest request)
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

        _logger.LogInformation("{LogPrefix} Verificando existência do item de estoque para remoção de quantidade...", UseCaseLogPrefix);
        var itemEstoqueResult = await _estoqueRepository.GetItemEstoqueByIdAsync(request.ItemId);
        if (itemEstoqueResult.IsFailed)
        {
            _logger.LogInformation("{LogPrefix} Item de estoque não encontrado para remoção de quantidade. ItemId: {ItemId}", UseCaseLogPrefix, request.ItemId);
            return Result.Fail(new NotFoundError("Item de estoque não encontrado para remoção de quantidade."));
        }
        var itemEstoque = itemEstoqueResult.Value;
        _logger.LogInformation("{LogPrefix} Item de estoque encontrado para remoção de quantidade. ItemId: {ItemId}", UseCaseLogPrefix, request.ItemId);

        _logger.LogInformation("{LogPrefix} Removendo quantidade do item de estoque...", UseCaseLogPrefix);
        itemEstoque.QuantidadeAtual -= request.QuantidadeARemover;
        _logger.LogInformation("{LogPrefix} Quantidade removida. ItemId: {ItemId}, QuantidadeARemover: {QuantidadeARemover}, NovaQuantidade: {NovaQuantidade}", UseCaseLogPrefix, request.ItemId, request.QuantidadeARemover, itemEstoque.QuantidadeAtual);

        _logger.LogInformation("{LogPrefix} Atualizando item de estoque no repositório...", UseCaseLogPrefix);
        await _estoqueRepository.UpdateItemEstoqueAsync(itemEstoque);
        _logger.LogInformation("{LogPrefix} Quantidade removida e item de estoque atualizado com sucesso. ItemId: {ItemId}, NovaQuantidade: {NovaQuantidade}", UseCaseLogPrefix, request.ItemId, itemEstoque.QuantidadeAtual);

        _logger.LogInformation("{LogPrefix} Execução do caso de uso concluída com sucesso.", UseCaseLogPrefix);
        return Result.Ok();
    }
}
