using AutoMapper;
using CakeGestao.Application.Features.Estoque.Common;
using CakeGestao.Application.UseCases.Estoque.Common;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Estoque.Query.Alerta;

public class GetAlertaEstoqueHandler : IRequestHandler<GetAlertaEstoqueQuery, Result<List<ItemEstoqueResponseAlert>>>
{
    private readonly IEstoqueRepository _estoqueRepository;
    private readonly ILogger<GetAlertaEstoqueHandler> _logger;
    private readonly IValidator<GetAlertaEstoqueQuery> _validator;
    private readonly IMapper _mapper;
    private const string LogPrefix = "[Get Alerta Estoque Handler]";

    public GetAlertaEstoqueHandler(IEstoqueRepository estoqueRepository, ILogger<GetAlertaEstoqueHandler> logger, IValidator<GetAlertaEstoqueQuery> validator, IMapper mapper)
    {
        _estoqueRepository = estoqueRepository;
        _logger = logger;
        _validator = validator;
        _mapper = mapper;
    }

    public async Task<Result<List<ItemEstoqueResponseAlert>>> Handle(GetAlertaEstoqueQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Verificando alertas de estoque baixo. EmpresaID: {EmpresaId}", LogPrefix, request.EmpresaId);

        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("{LogPrefix} Requisição inválida. EmpresaID: {EmpresaId}. Erros: {Errors}", LogPrefix, request.EmpresaId, string.Join(", ", errors));
            return Result.Fail(new ValidationError(errors));
        }

        var itensEstoqueResult = await _estoqueRepository.GetAlertaEstoqueByEmpresaIdAsync(request.EmpresaId, 5);
        if (itensEstoqueResult.IsFailed)
        {
            _logger.LogInformation("{LogPrefix} Nenhum item em estado de alerta no momento.", LogPrefix);
            return Result.Ok(new List<ItemEstoqueResponseAlert>());
        }

        var itensEstoqueResponse = _mapper.Map<List<ItemEstoqueResponseAlert>>(itensEstoqueResult.Value);

        _logger.LogInformation("{LogPrefix} Alertas encontrados. Total de itens críticos: {Count}", LogPrefix, itensEstoqueResponse.Count);
        return Result.Ok(itensEstoqueResponse);
    }
}
