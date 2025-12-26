using AutoMapper;
using CakeGestao.Application.Features.Empresas.Common;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Empresas.GetAll;

public class GetAllEmpresaHandler : IRequestHandler<GetAllEmpresaQuery, Result<List<EmpresaResponse>>>
{
    private readonly IEmpresaRepository _empresaRepository;
    private readonly ILogger<GetAllEmpresaHandler> _logger;
    private readonly IMapper _mapper;
    private const string UseCaseLogPrefix = "[Get All Empresas]";

    public GetAllEmpresaHandler(IEmpresaRepository empresaRepository, ILogger<GetAllEmpresaHandler> logger, IMapper mapper)
    {
        _empresaRepository = empresaRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<List<EmpresaResponse>>> Handle(GetAllEmpresaQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{UseCaseLogPrefix} Iniciando processo de busca de todas as empresas", UseCaseLogPrefix);

        var listEmpresaResult = await _empresaRepository.GetAllEmpresasAsync();
        if (listEmpresaResult.IsFailed)
        {
            _logger.LogWarning("{UseCaseLogPrefix} Falha ao buscar empresas no banco de dados", UseCaseLogPrefix);
            return Result.Fail(listEmpresaResult.Errors);
        }
        _logger.LogInformation("{UseCaseLogPrefix} Busca realizada com sucesso. {Count} empresas encontradas", UseCaseLogPrefix, listEmpresaResult.Value.Count);

        _logger.LogInformation("{UseCaseLogPrefix} Iniciando mapeamento das entidades para EmpresaResponse", UseCaseLogPrefix);
        var listEmpresa = _mapper.Map<List<EmpresaResponse>>(listEmpresaResult.Value);
        _logger.LogInformation("{UseCaseLogPrefix} Mapeamento concluído com sucesso", UseCaseLogPrefix);

        _logger.LogInformation("{UseCaseLogPrefix} Processo finalizado com sucesso", UseCaseLogPrefix);
        return Result.Ok(listEmpresa);
    }
}