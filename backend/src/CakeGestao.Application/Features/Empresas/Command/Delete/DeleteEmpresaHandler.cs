using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Empresas.Delete;

public class DeleteEmpresaHandler : IRequestHandler<DeleteEmpresaCommand, Result>
{
    private readonly ILogger<DeleteEmpresaHandler> _logger;
    private readonly IEmpresaRepository _empresaRepository;
    private const string LogPrefix = "[Delete Empresa Handler]";

    public DeleteEmpresaHandler(ILogger<DeleteEmpresaHandler> logger, IEmpresaRepository empresaRepository)
    {
        _logger = logger;
        _empresaRepository = empresaRepository;
    }

    public async Task<Result> Handle(DeleteEmpresaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Solicitada exclusão de empresa. ID: {Id}", LogPrefix, request.Id);

        var empresaResult = await _empresaRepository.GetEmpresaByIdAsync(request.Id);
        if (empresaResult.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Tentativa de exclusão falhou: Empresa não encontrada. ID: {Id}", LogPrefix, request.Id);
            return Result.Fail(empresaResult.Errors);
        }

        await _empresaRepository.DeleteEmpresaAsync(empresaResult.Value);

        _logger.LogInformation("{LogPrefix} Empresa excluída permanentemente. ID: {Id}", LogPrefix, request.Id);
        return Result.Ok();
    }
}
