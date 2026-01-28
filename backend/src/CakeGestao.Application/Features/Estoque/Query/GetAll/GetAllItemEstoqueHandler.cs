using AutoMapper;
using CakeGestao.Application.UseCases.Estoque.Common;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Estoque.Query.GetAll;

public class GetAllItemEstoqueHandler : IRequestHandler<GetAllItemEstoqueQuery, Result<List<ItemEstoqueResponse>>>
{
    private readonly IMapper _mapper;
    private readonly IEstoqueRepository _estoqueRepository;
    private readonly ILogger<GetAllItemEstoqueHandler> _logger;
    private readonly IValidator<GetAllItemEstoqueQuery> _validator;
    private const string LogPrefix = "[Get All Estoque Handler]";

    public GetAllItemEstoqueHandler(IMapper mapper, IEstoqueRepository estoqueRepository, ILogger<GetAllItemEstoqueHandler> logger, IValidator<GetAllItemEstoqueQuery> validator)
    {
        _mapper = mapper;
        _estoqueRepository = estoqueRepository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result<List<ItemEstoqueResponse>>> Handle(GetAllItemEstoqueQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Iniciando listagem de estoque. EmpresaId: {EmpresaId}", LogPrefix, request.EmpresaId);

        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("{LogPrefix} Requisição inválida. EmpresaId: {EmpresaId}. Erros: {Errors}", LogPrefix, request.EmpresaId, string.Join(", ", errors));
            return Result.Fail(new ValidationError(errors));
        }

        var itensEstoqueResult = await _estoqueRepository.GetAllItemEstoqueByEmpresaIdAsync(request.EmpresaId);
        if (itensEstoqueResult.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Nenhum item encontrado no estoque. EmpresaId: {EmpresaId}", LogPrefix, request.EmpresaId);
            var erro = itensEstoqueResult.Errors.ToString();
            return Result.Fail(new NotFoundError(erro!));
        }

        var itensEstoqueResponse = _mapper.Map<List<ItemEstoqueResponse>>(itensEstoqueResult.Value);

        _logger.LogInformation("{LogPrefix} Listagem concluída. Total de itens: {Count}", LogPrefix, itensEstoqueResponse.Count);
        return Result.Ok(itensEstoqueResponse);
    }
}
