using CakeGestao.Application.UseCases.Empresas.Interface;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.Empresas.UseCase;

public class DeleteEmpresaUseCase : IDeleteEmpresaUseCase
{
    
    private readonly ILogger<DeleteEmpresaUseCase> _logger;
    private readonly IEmpresaRepository _empresaRepository;

    public DeleteEmpresaUseCase(ILogger<DeleteEmpresaUseCase> logger, IEmpresaRepository empresaRepository)
    {
        _logger = logger;
        _empresaRepository = empresaRepository;
    }

    public async Task<Result> ExecuteAsync(int id)
    {
        _logger.LogInformation("Iniciando processo de delete empresa de id: {Id}", id);

        _logger.LogInformation("Iniciando processo de verificação da existencia da empresa de id: {Id} para delete", id);
        var empresaResult = await _empresaRepository.GetEmpresaByIdAsync(id);
        if (empresaResult.IsFailed)
        {
            _logger.LogInformation("Empresa de id {Id} não encontrado no banco de dados para delete", id);
            return Result.Fail(new NotFoundError("Empresa não encontrada"));
        }
        _logger.LogInformation("Empresa encontrado com id {Id} no banco de dados para delete", id);

        _logger.LogInformation("Iniciando o processo de delete do banco de dado da empresa de id: {Id}", id);
        await _empresaRepository.DeleteEmpresaAsync(empresaResult.Value);

        _logger.LogInformation("Processo de delete da empresa de id: {Id} realizado com susseso", id);
        return Result.Ok();
    }
}
