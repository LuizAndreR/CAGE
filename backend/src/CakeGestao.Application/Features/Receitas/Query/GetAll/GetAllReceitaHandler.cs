using AutoMapper;
using CakeGestao.Application.Features.Receitas.Common;
using CakeGestao.Domain.Exceptions;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Receitas.Query.GetAll;

public class GetAllReceitaHandler : IRequestHandler<GetAllReceitaQuery, Result<List<ReceitaResponseAll>>>
{
    private readonly IReceitaRepository _receitaRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAllReceitaQuery> _validator;
    private readonly ILogger<GetAllReceitaHandler> _logger;
    private const string LogPrefix = "[Get All Receita Handler]";

    public GetAllReceitaHandler(IReceitaRepository receitaRepository, IMapper mapper, IValidator<GetAllReceitaQuery> validator, ILogger<GetAllReceitaHandler> logger)
    {
        _receitaRepository = receitaRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<ReceitaResponseAll>>> Handle(GetAllReceitaQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Iniciando busca de receitas para a Empresa: {EmpresaId}", LogPrefix, request.EmpresaId);

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(x => x.ErrorMessage).ToList();
            _logger.LogWarning("{LogPrefix} Requisição inválida. EmpresaId: {EmpresaId}. Erros: {Errors}", LogPrefix, request.EmpresaId, string.Join(", ", errors));
            return Result.Fail(new ValidationError(errors));
        }

        var receitasResult = await _receitaRepository.GetAllReceitasAsync(request.EmpresaId);
        if (receitasResult.Value.Count <= 0)
        {
            _logger.LogInformation("{LogPrefix} Nenhuma receita encontrada para a Empresa: {EmpresaId}", LogPrefix, request.EmpresaId);
            return Result.Fail(new NotFoundError("Nenhuma receita foi encontrada no banco de dados."));
        }
        
        var listReceitas = _mapper.Map<List<ReceitaResponseAll>>(receitasResult.Value);

        _logger.LogInformation("{LogPrefix} Busca concluída com sucesso. Retornando {Numero} receitas cadastradas.", LogPrefix, listReceitas.Count); 
        return Result.Ok(listReceitas);    
    }
}