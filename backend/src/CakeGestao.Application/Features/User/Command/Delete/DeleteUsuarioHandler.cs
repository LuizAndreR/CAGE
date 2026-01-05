using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.User.Command.Delete;

public class DeleteUsuarioHandler : IRequestHandler<DeleteUsuarioCommand, Result>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ILogger<DeleteUsuarioHandler> _logger;
    private const string UseCaseLogPrefix = "[Delete Usuario]";

    public DeleteUsuarioHandler(IUsuarioRepository usuarioRepository, ILogger<DeleteUsuarioHandler> logger)
    {
        _usuarioRepository = usuarioRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteUsuarioCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{UseCaseLogPrefix} Iniciando processo de exclusão. UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Buscando usuário no repositório. UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);
        var usuarioResult = await _usuarioRepository.GetByIdAsync(request.Id);
        if (usuarioResult.IsFailed)
        {
            _logger.LogWarning("{UseCaseLogPrefix} Usuário não encontrado. UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);
            return Result.Fail(new NotFoundError("Usuário não encontrado."));
        }
        _logger.LogInformation("{UseCaseLogPrefix} Usuário encontrado. UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Iniciando exclusão do usuário. UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);
        await _usuarioRepository.DeleteAsync(usuarioResult.Value);
        _logger.LogInformation("{UseCaseLogPrefix} Exclusão concluída com sucesso. UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Processo de exclusão finalizado. UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);
        return Result.Ok();
    }
}
