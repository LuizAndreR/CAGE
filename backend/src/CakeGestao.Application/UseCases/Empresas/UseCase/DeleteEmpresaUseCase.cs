using CakeGestao.Application.UseCases.Empresas.Interface;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.Empresas.UseCase;

public class DeleteEmpresaUseCase : IDeleteEmpresaUseCase
{

    private readonly ILogger<DeleteEmpresaUseCase> _logger;
    private readonly IEmpresaRepository _empresaRepository;
    private const string UseCaseLogPrefix = "[Delete Empresa]";

    public DeleteEmpresaUseCase(ILogger<DeleteEmpresaUseCase> logger, IEmpresaRepository empresaRepository)
    {
        _logger = logger;
        _empresaRepository = empresaRepository;
    }

    public async Task<Result> ExecuteAsync(int id)
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

        _logger.LogInformation("{UseCaseLogPrefix} Iniciando exclusão da empresa de id: {Id} do banco de dados", UseCaseLogPrefix, id);
        await _empresaRepository.DeleteEmpresaAsync(empresaResult.Value);
        _logger.LogInformation("{UseCaseLogPrefix} Exclusão da empresa de id: {Id} concluída com sucesso", UseCaseLogPrefix, id);

        _logger.LogInformation("{UseCaseLogPrefix} Processo para a empresa de id: {Id} finalizado com sucesso", UseCaseLogPrefix, id);
        return Result.Ok();
    }
}
