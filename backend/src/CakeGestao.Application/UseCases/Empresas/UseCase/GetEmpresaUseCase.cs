using AutoMapper;
using CakeGestao.Application.Dtos.Responses;
using CakeGestao.Application.UseCases.Empresas.Interface;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.Empresas.UseCase;

public class GetEmpresaUseCase : IGetEmpresaUseCase
{
    private readonly IEmpresaRepository _empresaRepository;
    private readonly ILogger<GetEmpresaUseCase> _logger;
    private readonly IMapper _mapper;
    private const string UseCaseLogPrefix = "[Get Empresa By Id]";

    public GetEmpresaUseCase(IEmpresaRepository empresaRepository, ILogger<GetEmpresaUseCase> logger, IMapper mapper)
    {
        _empresaRepository = empresaRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<EmpresaResponse>> ExecuteAsync(int id)
    {
        _logger.LogInformation("{UseCaseLogPrefix} Iniciando processo para a empresa de id: {Id}", UseCaseLogPrefix, id);

        _logger.LogInformation("{UseCaseLogPrefix} Buscando a empresa de id: {Id} no banco de dados", UseCaseLogPrefix, id);
        var empresaResult = await _empresaRepository.GetEmpresaByIdAsync(id);
        if (empresaResult.IsFailed)
        {
            _logger.LogWarning("{UseCaseLogPrefix} Empresa de id: {Id} não encontrada no banco de dados", UseCaseLogPrefix, id);
            return Result.Fail(empresaResult.Errors);
        }
        _logger.LogInformation("{UseCaseLogPrefix} Empresa de id: {Id} encontrada com sucesso", UseCaseLogPrefix, id);

        _logger.LogInformation("{UseCaseLogPrefix} Iniciando mapeamento da entidade para EmpresaResponse para a empresa de id: {Id}", UseCaseLogPrefix, id);
        var empresaResponse = _mapper.Map<EmpresaResponse>(empresaResult.Value);
        _logger.LogInformation("{UseCaseLogPrefix} Mapeamento para a empresa de id: {Id} concluído com sucesso", UseCaseLogPrefix, id);

        _logger.LogInformation("{UseCaseLogPrefix} Processo para a empresa de id: {Id} finalizado com sucesso", UseCaseLogPrefix, id);
        return Result.Ok(empresaResponse);
    }
}
