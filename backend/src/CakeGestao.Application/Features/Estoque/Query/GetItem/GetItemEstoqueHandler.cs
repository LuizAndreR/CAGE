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
    private const string LogPrefix = "[Get Item Estoque Handler]";

    public GetItemEstoqueHandler(IEstoqueRepository estoqueRepository, ILogger<GetItemEstoqueHandler> logger, IMapper mapper, IValidator<GetItemEstoqueQuery> validator)
    {
        _estoqueRepository = estoqueRepository;
        _logger = logger;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<Result<ItemEstoqueResponse>> Handle(GetItemEstoqueQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Buscando detalhes do item. ItemID: {ItemId} | EmpresaID: {EmpresaId}", LogPrefix, request.ItemId, request.EmpresaId);

        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("{LogPrefix} Requisição inválida. Erros: {Errors}", LogPrefix, string.Join(", ", errors));
            return Result.Fail(new ValidationError(errors));
        }

        var itemEstoqueResult = await _estoqueRepository.GetItemEstoqueByIdAsync(request.ItemId, request.EmpresaId);
        if (itemEstoqueResult.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Item não encontrado no banco. ItemID: {ItemId}", LogPrefix, request.ItemId);
            var erro = itemEstoqueResult.Errors.ToString();
            return Result.Fail(new NotFoundError(erro!));
        }
        var itemEstoque = itemEstoqueResult.Value;

        var itemEstoqueResponse = _mapper.Map<ItemEstoqueResponse>(itemEstoque);

        _logger.LogInformation("{LogPrefix} Dados retornados com sucesso. ID: {Id}", LogPrefix, request.ItemId);
        return Result.Ok(itemEstoqueResponse);
    }
}
