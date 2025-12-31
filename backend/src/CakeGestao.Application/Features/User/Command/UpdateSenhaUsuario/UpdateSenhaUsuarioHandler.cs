using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.User.Command.UpdateSenhaUsuario;

public class UpdateSenhaUsuarioHandler : IRequestHandler<UpdateSenhaUsuarioCommand, Result>
{
    private readonly IUsuarioRepository _repositoryUser;
    private readonly ILogger<UpdateSenhaUsuarioHandler> _logger;
    private readonly IValidator<UpdateSenhaUsuarioCommand> _validator;
    private const string UseCaseLogPrefix = "[Update Senha Usuario]" ;

    public UpdateSenhaUsuarioHandler(IUsuarioRepository repositoryUser, ILogger<UpdateSenhaUsuarioHandler> logger, IValidator<UpdateSenhaUsuarioCommand> validator)
    {
        _repositoryUser = repositoryUser;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result> Handle(UpdateSenhaUsuarioCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{UseCaseLogPrefix} Iniciando processo de atualização de senha. UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Validando requisição para UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);
        ValidationResult validationResult = _validator.Validate(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("{UseCaseLogPrefix} Validação falhou para UsuarioId: {UsuarioId}. Erros: {Errors}", UseCaseLogPrefix, request.Id, errors);
            return Result.Fail(new ValidationError(errors));
        }
        _logger.LogInformation("{UseCaseLogPrefix} Validação concluída com sucesso para UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Buscando usuário no repositório. UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);
        var usuarioResult = await _repositoryUser.GetByIdAsync(request.Id);
        if (usuarioResult.IsFailed)
        {
            _logger.LogWarning("{UseCaseLogPrefix} Usuário não encontrado. UsuarioId: {UsuarioId}. Erros: {@Errors}", UseCaseLogPrefix, request.Id, usuarioResult.Errors);
            return Result.Fail(new NotFoundError("Usuario nao foi encontrado no banco de dados"));
        }
        var usuario = usuarioResult.Value;
        _logger.LogInformation("{UseCaseLogPrefix} Usuário encontrado. UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Verificando senha atual para UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);
        if (!BCrypt.Net.BCrypt.Verify(request.SenhaAtual, usuario.SenhaHash))
        {
            _logger.LogWarning("{UseCaseLogPrefix} Senha atual inválida para UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);
            return Result.Fail(new ValidationError(new List<string> { "Senha atual inválida" }));
        }
        _logger.LogInformation("{UseCaseLogPrefix} Senha atual verificada com sucesso para UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Verificando se nova senha é diferente da atual para UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);
        if (BCrypt.Net.BCrypt.Verify(request.NovaSenha, usuario.SenhaHash))
        {
            _logger.LogWarning("{UseCaseLogPrefix} Nova senha igual à atual para UsuarioId: {UsuarioId}. Operação cancelada.", UseCaseLogPrefix, request.Id);
            return Result.Fail(new ConflictError("A nova senha não pode ser igual à senha atual"));
        }
        _logger.LogInformation("{UseCaseLogPrefix} Nova senha validada como diferente da atual para UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Iniciando atualização da senha em memória para UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);
        usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.NovaSenha);
        _logger.LogInformation("{UseCaseLogPrefix} Atualização da senha em memória concluída para UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Iniciando persistência da nova senha para UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);
        await _repositoryUser.UpdateUsuarioAsync(usuario);
        _logger.LogInformation("{UseCaseLogPrefix} Persistência da nova senha concluída com sucesso para UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Processo de atualização de senha finalizado com sucesso para UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);
        return Result.Ok();
    }
}
