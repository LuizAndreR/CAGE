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

    public GetEmpresaUseCase(IEmpresaRepository empresaRepository, ILogger<GetEmpresaUseCase> logger, IMapper mapper)
    {
        _empresaRepository = empresaRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<EmpresaResponse>> ExecuteAsync(int id)
    {
        _logger.LogInformation("Iniciando o processo de get de uma empresa pelo id: {Id}", id);

        _logger.LogInformation("Buscando a empresa no repositório pelo id: {Id}", id);
        var empresaResult = await _empresaRepository.GetEmpresaByIdAsync(id);
        if (empresaResult.IsFailed)
        {
            _logger.LogWarning("Empresa com id: {Id} não encontrada no repositório", id);
            return Result.Fail(new NotFoundError($"Empresa com id: {id} não encontrada."));
        }
        _logger.LogInformation("Empresa com id: {Id} encontrada com sucesso no repositório", id);

        _logger.LogInformation("Mapeando a entidade Empresa para EmpresaResponse");
        var empresaResponse = _mapper.Map<EmpresaResponse>(empresaResult.Value);
        _logger.LogInformation("Mapeamento concluído com sucesso");

        _logger.LogInformation("Processo de get de empresa concluído com sucesso para o id: {Id}", id);
        return Result.Ok(empresaResponse);
    }
}
