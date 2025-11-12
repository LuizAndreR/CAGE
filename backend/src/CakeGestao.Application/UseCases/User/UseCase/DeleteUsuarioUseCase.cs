using CakeGestao.Application.UseCases.User.Interface;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.User.UseCase;

public class DeleteUsuarioUseCase : IDeleteUsuarioUseCase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ILogger<DeleteUsuarioUseCase> _logger;

    public DeleteUsuarioUseCase(IUsuarioRepository usuarioRepository, ILogger<DeleteUsuarioUseCase> logger)
    {
        _usuarioRepository = usuarioRepository;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(int usuarioId)
    {
        _logger.LogInformation("Iniciando o processo de exclusão do usuário com ID: {UsuarioId}", usuarioId);
        
        _logger.LogInformation("Verificando a existência do usuário com ID: {UsuarioId}", usuarioId);
        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
        if (usuario == null)
        {
            _logger.LogWarning("Usuário com ID: {UsuarioId} não encontrado. Processo de exclusão abortado.", usuarioId);
            return Result.Fail(new NotFoundError("Usuário não encontrado."));
        }
        _logger.LogInformation("Usuário com ID: {UsuarioId} encontrado. Prosseguindo com a exclusão.", usuarioId);

        _logger.LogInformation("Excluindo o usuário com ID: {UsuarioId}", usuarioId);
        await _usuarioRepository.DeleteAsync(usuario.Value);
        _logger.LogInformation("Usuário com ID: {UsuarioId} excluído com sucesso.", usuarioId);
        
        _logger.LogInformation("Processo de exclusão do usuário com ID: {UsuarioId} concluído com sucesso.", usuarioId);
        return Result.Ok();
    }
}
