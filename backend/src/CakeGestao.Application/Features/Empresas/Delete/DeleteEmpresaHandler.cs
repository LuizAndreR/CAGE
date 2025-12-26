using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Empresas.Delete;

public class DeleteEmpresaHandler : IRequestHandler<DeleteEmpresaCommand, Result>
{
    private readonly ILogger<DeleteEmpresaHandler> _logger;
    private readonly IEmpresaRepository _empresaRepository;
    private const string UseCaseLogPrefix = "[Delete Empresa]";

    public DeleteEmpresaHandler(ILogger<DeleteEmpresaHandler> logger, IEmpresaRepository empresaRepository)
    {
        _logger = logger;
        _empresaRepository = empresaRepository;
    }

    public async Task<Result> Handle(DeleteEmpresaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{UseCaseLogPrefix} Iniciando processo para a empresa de id: {Id}", UseCaseLogPrefix, request.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Buscando a empresa de id: {Id} no banco de dados", UseCaseLogPrefix, request.Id);
        var empresaResult = await _empresaRepository.GetEmpresaByIdAsync(request.Id);
        if (empresaResult.IsFailed)
        {
            _logger.LogWarning("{UseCaseLogPrefix} Empresa de id: {Id} não encontrada no banco de dados", UseCaseLogPrefix, request.Id);
            return Result.Fail(empresaResult.Errors);
        }
        _logger.LogInformation("{UseCaseLogPrefix} Empresa de id: {Id} encontrada com sucesso", UseCaseLogPrefix, request.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Iniciando exclusão da empresa de id: {Id} do banco de dados", UseCaseLogPrefix, request.Id);
        await _empresaRepository.DeleteEmpresaAsync(empresaResult.Value);
        _logger.LogInformation("{UseCaseLogPrefix} Exclusão da empresa de id: {Id} concluída com sucesso", UseCaseLogPrefix, request.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Processo para a empresa de id: {Id} finalizado com sucesso", UseCaseLogPrefix, request.Id);
        return Result.Ok();
    }
}
