using AutoMapper;
using CakeGestao.Application.Features.Receitas.Common;
using CakeGestao.Domain.Exceptions;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Receitas.Query.GetReceita;

public class GetReceitaHandler : IRequestHandler<GetReceitaQuery, Result<ReceitaResponse>>
{
    private readonly ILogger<GetReceitaHandler> _logger;
    private readonly IReceitaRepository _receitaRepository;
    private readonly IValidator<GetReceitaQuery> _validator;
    private readonly IMapper _mapper;
    private const string LogPrefix = "[Get Receita Handler]";

    public GetReceitaHandler(ILogger<GetReceitaHandler> logger, IReceitaRepository receitaRepository, IValidator<GetReceitaQuery> validator, IMapper mapper)
    {
        _logger = logger;
        _receitaRepository = receitaRepository;
        _validator = validator;
        _mapper = mapper;
    }

    public async Task<Result<ReceitaResponse>> Handle(GetReceitaQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Buscando receita ID {Id} para a Empresa {EmpresaId}", LogPrefix, request.Id, request.EmpresaId);

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("{LogPrefix} Validação falhou para receita ID {Id}. Erros: {Errors}", LogPrefix, request.Id, validationResult.Errors);
            return Result.Fail(new ValidationError(validationResult.Errors.Select(e => e.ErrorMessage).ToList()));
        }

        var resultRepository = await _receitaRepository.GetReceitaByIdAsync(request.Id, request.EmpresaId);
        if (resultRepository.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Receita ID {Id} não encontrada ou não pertence à empresa {EmpresaId}", LogPrefix, request.Id, request.EmpresaId);
            return Result.Fail(new NotFoundError($"Receita de id {request.Id} não encontrada."));
        }

        var receitaResponse = _mapper.Map<ReceitaResponse>(resultRepository.Value);

        _logger.LogInformation("{LogPrefix} Dados retornados com sucesso. ID: {Id}", LogPrefix, request.Id);
        return Result.Ok(receitaResponse);
    }
}
