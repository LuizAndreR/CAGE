using CakeGestao.Application.Dtos.Requests.Usuario;
using CakeGestao.Application.UseCases.User.Interface;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.User.UseCase;

public class UpdateUserUseCase : IUpdateUserUseCase
{
    private readonly IUsuarioRepository _repositoryUser; 
    private readonly IValidator<UpdateUsuarioRequest> _validator;
    private readonly ILogger<UpdateUserUseCase> _logger;

    public UpdateUserUseCase(IUsuarioRepository repositoryUser, IValidator<UpdateUsuarioRequest> validator, ILogger<UpdateUserUseCase> logger)
    {
        _repositoryUser = repositoryUser;
        _validator = validator;
        _logger = logger;
    }


    public async Task<Result> ExecuteAsync(UpdateUsuarioRequest request, int id)
    {
        _logger.LogInformation("Iniciando o processo de UpdateUser com o usuario de Id {Id}", id);
        
        _logger.LogInformation("Iniciando o processo de verificação da request de update do usuario de id: {Id}", id);
        var validationResult = _validator.Validate(request);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validação falhou para update do usuario {Id}", id);
            return Result.Fail(new ValidationError(validationResult.Errors.Select(e => e.ErrorMessage).ToList()));
        }
        _logger.LogInformation("Validação realizada com sucesso para update do usuario {Id}", id);
        
        _logger.LogInformation("Iniciando o processo de verificação da existencia do usuario de id: {Id}", id);
        var usuarioResult = await _repositoryUser.GetByIdAsync(id);
        if (usuarioResult.IsFailed)
        {
            _logger.LogWarning("Usuario nao foi encontrado no banco de dados {Id}", id);
            return Result.Fail(new NotFoundError("Usuario nao foi encontrado no banco de dados"));
        }
        var usuario = usuarioResult.Value;
        _logger.LogInformation("Processo de verificação realizada com susseso da existencia do usuario de id: {Id}", id);
        
        _logger.LogInformation("Iniciando o processo de verificação das alterações");
        if (usuario.Nome == request.Nome || usuario.Email == request.Email)
        {
            _logger.LogInformation("As alterações ja foram realizado anteriomente com usuario {Id}", id);
            return Result.Ok();
        }
        _logger.LogInformation("Processo de verificação das alterações relizado com sucesso com usuario {Id}", id);
        
        _logger.LogInformation("Iniciando o processo de atulização do usuario  {Id}", id);
        usuario.Nome = request.Nome;
        usuario.Email = request.Email;
        _logger.LogInformation("Processo de atulização realizado com susseso do usuario {Id}", id);
        
        await _repositoryUser.UpdateUsuarioAsync(usuario);
        
        _logger.LogInformation("Processo realizado com susseso no update do usuario:  {Id}", id);
        return Result.Ok();
    }
}