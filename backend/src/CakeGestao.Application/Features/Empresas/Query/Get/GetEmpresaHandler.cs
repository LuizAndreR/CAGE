using AutoMapper;
using CakeGestao.Application.Features.Empresas.Common;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Empresas.Query.Get;

public class GetEmpresaHandler : IRequestHandler<GetEmpresaQuery, Result<EmpresaResponse>>
{
    private readonly IEmpresaRepository _empresaRepository;
    private readonly ILogger<GetEmpresaHandler> _logger;
    private readonly IMapper _mapper;
    private const string LogPrefix = "[Get Empresa Handler]";

    public GetEmpresaHandler(IEmpresaRepository empresaRepository, ILogger<GetEmpresaHandler> logger, IMapper mapper)
    {
        _empresaRepository = empresaRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<EmpresaResponse>> Handle(GetEmpresaQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Buscando detalhes da empresa. ID: {Id}", LogPrefix, request.Id);

        var empresaResult = await _empresaRepository.GetEmpresaByIdAsync(request.Id);
        if (empresaResult.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Empresa não encontrada. ID: {Id}", LogPrefix, request.Id);
            return Result.Fail(empresaResult.Errors);
        }

        var empresaResponse = _mapper.Map<EmpresaResponse>(empresaResult.Value);

        _logger.LogInformation("{LogPrefix} Dados retornados com sucesso. ID: {Id}", LogPrefix, request.Id);
        return Result.Ok(empresaResponse);
    }
}
