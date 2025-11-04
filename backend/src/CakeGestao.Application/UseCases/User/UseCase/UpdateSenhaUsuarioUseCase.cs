using CakeGestao.Application.Dtos.Requests.Usuario;
using CakeGestao.Application.UseCases.User.Interface;
using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.User.UseCase;

public class UpdateSenhaUsuarioUseCase : IUpdateSenhaUsuarioUseCase
{
    private readonly IUsuarioRepository _repositoryUser;
    private readonly ILogger<UpdateSenhaUsuarioUseCase> _logger;
    private readonly IValidator<UpdateSenhaUsuarioRequest> _validator;

    public UpdateSenhaUsuarioUseCase(IUsuarioRepository repositoryUser, ILogger<UpdateSenhaUsuarioUseCase> logger, IValidator<UpdateSenhaUsuarioRequest> validator)
    {
        _repositoryUser = repositoryUser;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result> ExecuteAsync(UpdateSenhaUsuarioRequest request, int id)
    {
        _logger.LogInformation("Iniciando o processo de UpdateSenhaUsuarioUseCase com o usuario de Id {Id}", id);

        _logger.LogInformation("Iniciando o processo de verificação da request de update da senha do usuario de id: {Id}", id);
        var validationResult = _validator.Validate(request);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validação falhou para update da senha do usuario {Id}", id);
            throw new ValidationError(validationResult.Errors.Select(e => e.ErrorMessage).ToList());
        }
        _logger.LogInformation("Validação realizada com sucesso para update da senha do usuario {Id}", id);

        _logger.LogInformation("Iniciando o processo de verificação da existencia do usuario de id: {Id}", id);
        var usuarioResult = await _repositoryUser.GetByIdAsync(id);
        if (usuarioResult.IsFailed)
        {
            _logger.LogWarning("Usuario nao foi encontrado no banco de dados {Id}", id);
            throw new NotFoundError("Usuario nao foi encontrado no banco de dados");
        }
        var usuario = usuarioResult.Value;
        _logger.LogInformation("Processo de verificação realizada com susseso da existencia do usuario de id: {Id}", id);

        _logger.LogInformation("Iniciando o processo de verificação da senha atual do usuario de id: {Id}", id);
        if (!BCrypt.Net.BCrypt.Verify(request.SenhaAtual, usuario.SenhaHash))
        {
            _logger.LogInformation("Senha atual inválida para o usuario {Id}", id);
            throw new ValidationError(new List<string> { "Senha atual inválida" });
        }
        _logger.LogInformation("Senha atual verificada com sucesso para o usuario de id: {Id}", id);

        _logger.LogInformation("Iniciando o processo de verificação da nova senha do usuario de id: {Id}", id);
        if (BCrypt.Net.BCrypt.Verify(request.NovaSenha, usuario.SenhaHash))
        {
            _logger.LogInformation("A nova senha é igual a senha atual para o usuario {Id}", id);
            throw new ConflictError("A nova senha é igual ser igual a senha atual");
        }
        _logger.LogInformation("Nova senha verificada com sucesso para o usuario de id: {Id}", id);

        _logger.LogInformation("Iniciando o processo de atualização da senha do usuario de id: {Id}", id);
        usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.NovaSenha);
        await _repositoryUser.UpdateUsuarioAsync(usuario);
        _logger.LogInformation("Processo de atualização da senha realizado com susseso do usuario {Id}", id);

        _logger.LogInformation("Senha do usuario {Id} atualizada com sucesso", id);
        return Result.Ok();
    }
}
