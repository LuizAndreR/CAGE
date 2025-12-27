using AutoMapper;
using CakeGestao.Application.UseCases.Estoque.Common;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Estoque.Query.GetItem;

public class GetItemEstoqueHandler : IRequestHandler<GetItemEstoqueQuery, Result<ItemEstoqueResponse>>
{
    private readonly IEstoqueRepository _estoqueRepository;
    private readonly ILogger<GetItemEstoqueHandler> _logger;
    private readonly IMapper _mapper;
    private readonly IValidator<GetItemEstoqueQuery> _validator;
    private const string UseCaseLogPrefix = "[Get Item Estoque]";

    public GetItemEstoqueHandler(IEstoqueRepository estoqueRepository, ILogger<GetItemEstoqueHandler> logger, IMapper mapper, IValidator<GetItemEstoqueQuery> validator)
    {
        _estoqueRepository = estoqueRepository;
        _logger = logger;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<Result<ItemEstoqueResponse>> Handle(GetItemEstoqueQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Iniciando execução do caso de uso com EmpresaId: {EmpresaId}, ItemId: {ItemId}", UseCaseLogPrefix, request.EmpresaId, request.ItemId);

        _logger.LogInformation("{LogPrefix} Validando requisição...", UseCaseLogPrefix);
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("{LogPrefix} Validação falhou: {Errors}", UseCaseLogPrefix, validationResult.Errors);
            return Result.Fail<ItemEstoqueResponse>("Requisição inválida").WithErrors(validationResult.Errors.Select(e => e.ErrorMessage));
        }
        _logger.LogInformation("{LogPrefix} Requisição validada com sucesso.", UseCaseLogPrefix);

        _logger.LogInformation("{LogPrefix} Buscando item de estoque no repositório...", UseCaseLogPrefix);
        var itemEstoqueResult = await _estoqueRepository.GetItemEstoqueByIdAsync(request.ItemId);
        if (itemEstoqueResult.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Item de estoque não encontrado para EmpresaId: {EmpresaId}, ItemId: {ItemId}", UseCaseLogPrefix, request.EmpresaId, request.ItemId);
            return Result.Fail<ItemEstoqueResponse>("Item de estoque não encontrado").WithErrors(itemEstoqueResult.Errors);
        }
        var itemEstoque = itemEstoqueResult.Value;
        _logger.LogInformation("{LogPrefix} Item de estoque encontrado com sucesso.", UseCaseLogPrefix);

        _logger.LogInformation("{LogPrefix} Mapeando entidade para DTO de resposta...", UseCaseLogPrefix);
        var itemEstoqueResponse = _mapper.Map<ItemEstoqueResponse>(itemEstoque);
        _logger.LogInformation("{LogPrefix} Mapeamento concluído com sucesso.", UseCaseLogPrefix);

        _logger.LogInformation("{LogPrefix} Execução do caso de uso concluída com sucesso.", UseCaseLogPrefix);
        return Result.Ok(itemEstoqueResponse);
    }
}
