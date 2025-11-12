using CakeGestao.Application.Dtos.Requests.Usuario;
using CakeGestao.Application.UseCases.User.Interface;
using CakeGestao.Domain.Enun;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.User.UseCase;

public class UpdateFuncionarioUseCase : IUpdateFuncionarioUseCase
{
    private readonly ILogger<UpdateFuncionarioUseCase> _logger;   
    private readonly IUsuarioRepository _repositoryUser;
    private readonly IValidator<UpdateFuncionarioUsuarioRequest> _validator;

    public UpdateFuncionarioUseCase(ILogger<UpdateFuncionarioUseCase> logger, IUsuarioRepository repositoryUser, IValidator<UpdateFuncionarioUsuarioRequest> validator)
    {
        _logger = logger;
        _repositoryUser = repositoryUser;
        _validator = validator;
    }

    public async Task<Result> ExecuteAsync(UpdateFuncionarioUsuarioRequest request)
    {
        _logger.LogInformation("Iniciando o processo de UpdateUsuarioFuncao do usuario com id: {Id}", request.Id);

        _logger.LogInformation("Iniciando o processo de verificação da request de update da função do usuario de id: {Id}", request.Id);
        var validationResult = _validator.Validate(request);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validação falhou para update da função do usuario {Id}", request.Id);
            return Result.Fail(new ValidationError(validationResult.Errors.Select(e => e.ErrorMessage).ToList()));
        }
        _logger.LogInformation("Validação realizada com sucesso para update da função do usuario {Id}", request.Id);

        _logger.LogInformation("Iniciando o processo de verificação da existencia do usuario de id: {Id}", request.Id);
        var usuarioResult = await _repositoryUser.GetByIdAsync(request.Id);
        if (usuarioResult.IsFailed)
        {
            _logger.LogWarning("Usuario nao foi encontrado no banco de dados {Id}", request.Id);
            return Result.Fail(new NotFoundError("Usuario nao foi encontrado no banco de dados"));
        }
        var usuario = usuarioResult.Value;
        _logger.LogInformation("Processo de verificação realizada com susseso da existencia do usuario de id: {Id}", request.Id);

        _logger.LogInformation("Iniciando o processo de verificação da role do updade do usuario {Id}", request.Id);
        if (!Enum.TryParse<UserRole>(request.Role, true, out var userRole))
        {
            _logger.LogWarning("Role inválida fornecida para updade do usuario {Id}", request.Id);
            return Result.Fail(new ValidationError(new List<string> { "Role inválida" }));
        }
        _logger.LogInformation("Role verificado com sucesso para updade do usuario {Id}", request.Id);

        _logger.LogInformation("Iniciando o processo de verificação das alterações");
        if (usuario.Nome == request.Nome && usuario.Role.ToString() == request.Role)
        {
            _logger.LogInformation("As alterações ja foram realizado anteriomente com usuario {Id}", request.Id);
            return Result.Ok();
        }
        _logger.LogInformation("Processo de verificação das alterações relizado com sucesso com usuario {Id}", request.Id);

        _logger.LogInformation("Iniciando o processo de atulização da função do usuario  {Id}", request.Id);
        usuario.Nome = request.Nome;
        usuario.Role = Enum.Parse<UserRole>(request.Role);
        _logger.LogInformation("Processo de atulização realizado com susseso da função do usuario {Id}", request.Id);

        _logger.LogInformation("Iniciando o processo de atualização da função do usuario de id: {Id}", request.Id);
        await _repositoryUser.UpdateUsuarioAsync(usuario);
        _logger.LogInformation("Processo de atualização da função realizado com susseso do usuario {Id}", request.Id);

        _logger.LogInformation("UpdateUsuarioFuncaoUseCase finalizado com sucesso para o usuario de id: {Id}", request.Id);
        return Result.Ok();

    }
}
