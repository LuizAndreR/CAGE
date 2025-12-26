using CakeGestao.Application.UseCases.User.Interface;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.User.UseCase;

public class DeleteUsuarioUseCase : IDeleteUsuarioUseCase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ILogger<DeleteUsuarioUseCase> _logger;
    private const string UseCaseLogPrefix = "[Delete Usuario]";

    public DeleteUsuarioUseCase(IUsuarioRepository usuarioRepository, ILogger<DeleteUsuarioUseCase> logger)
    {
        _usuarioRepository = usuarioRepository;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(int usuarioId)
    {
        _logger.LogInformation("{UseCaseLogPrefix} Iniciando processo de exclusão. UsuarioId: {UsuarioId}", UseCaseLogPrefix, usuarioId);

        _logger.LogInformation("{UseCaseLogPrefix} Buscando usuário no repositório. UsuarioId: {UsuarioId}", UseCaseLogPrefix, usuarioId);
        var usuarioResult = await _usuarioRepository.GetByIdAsync(usuarioId);
        if (usuarioResult.IsFailed)
        {
            _logger.LogWarning("{UseCaseLogPrefix} Usuário não encontrado. UsuarioId: {UsuarioId}", UseCaseLogPrefix, usuarioId);
            return Result.Fail(new NotFoundError("Usuário não encontrado."));
        }
        _logger.LogInformation("{UseCaseLogPrefix} Usuário encontrado. UsuarioId: {UsuarioId}", UseCaseLogPrefix, usuarioId);

        _logger.LogInformation("{UseCaseLogPrefix} Iniciando exclusão do usuário. UsuarioId: {UsuarioId}", UseCaseLogPrefix, usuarioId);
        await _usuarioRepository.DeleteAsync(usuarioResult.Value);
        _logger.LogInformation("{UseCaseLogPrefix} Exclusão concluída com sucesso. UsuarioId: {UsuarioId}", UseCaseLogPrefix, usuarioId);

        _logger.LogInformation("{UseCaseLogPrefix} Processo de exclusão finalizado. UsuarioId: {UsuarioId}", UseCaseLogPrefix, usuarioId);
        return Result.Ok();
    }
}
