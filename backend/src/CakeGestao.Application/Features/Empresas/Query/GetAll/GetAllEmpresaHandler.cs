using AutoMapper;
using CakeGestao.Application.Features.Empresas.Common;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Empresas.Query.GetAll;

public class GetAllEmpresaHandler : IRequestHandler<GetAllEmpresaQuery, Result<List<EmpresaResponse>>>
{
    private readonly IEmpresaRepository _empresaRepository;
    private readonly ILogger<GetAllEmpresaHandler> _logger;
    private readonly IMapper _mapper;
    private const string LogPrefix = "[Get All Empresas Handler]";

    public GetAllEmpresaHandler(IEmpresaRepository empresaRepository, ILogger<GetAllEmpresaHandler> logger, IMapper mapper)
    {
        _empresaRepository = empresaRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<List<EmpresaResponse>>> Handle(GetAllEmpresaQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Iniciando listagem de todas as empresas cadastradas.", LogPrefix);

        var listEmpresaResult = await _empresaRepository.GetAllEmpresasAsync();
        if (listEmpresaResult.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Falha ao buscar lista de empresas.", LogPrefix);
            return Result.Fail(listEmpresaResult.Errors);
        }
        var listEmpresa = _mapper.Map<List<EmpresaResponse>>(listEmpresaResult.Value);

        _logger.LogInformation("{LogPrefix} Listagem concluída com sucesso. Total de registros: {Count}", LogPrefix, listEmpresaResult.Value.Count);
        return Result.Ok(listEmpresa);
    }
}