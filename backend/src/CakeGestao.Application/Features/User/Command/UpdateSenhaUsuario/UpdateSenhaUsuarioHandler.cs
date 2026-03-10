using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.User.Command.UpdateSenhaUsuario;

public class UpdateSenhaUsuarioHandler : IRequestHandler<UpdateSenhaUsuarioCommand, Result>
{
    private readonly IUsuarioRepository _repositoryUser;
    private readonly ILogger<UpdateSenhaUsuarioHandler> _logger;
    private readonly IValidator<UpdateSenhaUsuarioCommand> _validator;
    private const string LogPrefix = "[Update Senha Handler]";

    public UpdateSenhaUsuarioHandler(IUsuarioRepository repositoryUser, ILogger<UpdateSenhaUsuarioHandler> logger, IValidator<UpdateSenhaUsuarioCommand> validator)
    {
        _repositoryUser = repositoryUser;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result> Handle(UpdateSenhaUsuarioCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Iniciando alteração de senha. ID: {Id}", LogPrefix, request.Id);

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("{LogPrefix} Dados inválidos. ID: {Id}. Erros: {Errors}", LogPrefix, request.Id, string.Join(", ", errors));
            return Result.Fail(new ValidationError(errors));
        }

        var usuarioResult = await _repositoryUser.GetByIdAsync(request.Id, request.EmpresaId);
        if (usuarioResult.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Usuário não encontrado. ID: {Id}", LogPrefix, request.Id);
            return Result.Fail(new NotFoundError("Usuario nao foi encontrado no banco de dados"));
        }
        var usuario = usuarioResult.Value;

        if (!BCrypt.Net.BCrypt.Verify(request.SenhaAtual, usuario.SenhaHash))
        {
            _logger.LogWarning("{LogPrefix} Falha na alteração: Senha atual incorreta. ID: {Id}", LogPrefix, request.Id);
            return Result.Fail(new ValidationError(new List<string> { "Senha atual inválida" }));
        }

        var senhaHash = BCrypt.Net.BCrypt.HashPassword(request.NovaSenha);
        usuario.AlterarSenhaHash(senhaHash);

        await _repositoryUser.UpdateUsuarioAsync(usuario);

        _logger.LogInformation("{LogPrefix} Senha alterada com sucesso. ID: {Id}", LogPrefix, request.Id);
        return Result.Ok();
    }
}
