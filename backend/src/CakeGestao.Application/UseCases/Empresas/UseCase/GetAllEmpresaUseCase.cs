using AutoMapper;
using CakeGestao.Application.Dtos.Responses;
using CakeGestao.Application.UseCases.Empresas.Interface;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.Empresas.UseCase;

public class GetAllEmpresaUseCase : IGetAllEmpresaUseCase
{
    private readonly IEmpresaRepository _empresaRepository;
    private readonly ILogger<GetAllEmpresaUseCase> _logger;
    private readonly IMapper _mapper;
    private const string UseCaseLogPrefix = "[Get All Empresas]";

    public GetAllEmpresaUseCase(IEmpresaRepository empresaRepository, ILogger<GetAllEmpresaUseCase> logger, IMapper mapper)
    {
        _empresaRepository = empresaRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<List<EmpresaResponse>>> ExecuteAsync()
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