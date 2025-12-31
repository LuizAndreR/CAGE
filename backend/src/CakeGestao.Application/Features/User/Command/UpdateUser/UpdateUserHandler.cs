using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.User.Command.UpdateUser;

public class UpdateUserHandler: IRequestHandler<UpdateUsuarioCommand, Result>
{
    private readonly IUsuarioRepository _repositoryUser; 
    private readonly IValidator<UpdateUsuarioCommand> _validator;
    private readonly ILogger<UpdateUserHandler> _logger;
    private const string UseCaseLogPrefix = "[Update Usuario]";

    public UpdateUserHandler(IUsuarioRepository repositoryUser, IValidator<UpdateUsuarioCommand> validator, ILogger<UpdateUserHandler> logger)
    {
        _repositoryUser = repositoryUser;
        _validator = validator;
        _logger = logger;
    }


    public async Task<Result> Handle(UpdateUsuarioCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{UseCaseLogPrefix} Iniciando processo de atualização do usuário. UsuarioId: {UsuarioId}, Email: {Email}", UseCaseLogPrefix, request.Id, request.Email);
        
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
        
        _logger.LogInformation("{UseCaseLogPrefix} Verificando se há alterações para UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);
        if (usuario.Nome == request.Nome && usuario.Email == request.Email)
        {
            _logger.LogWarning("{UseCaseLogPrefix} Nenhuma alteração detectada para UsuarioId: {UsuarioId}. Operação cancelada.", UseCaseLogPrefix, request.Id);
            return Result.Ok();
        }
        _logger.LogInformation("{UseCaseLogPrefix} Alterações detectadas para UsuarioId: {UsuarioId}. Prosseguindo com atualização.", UseCaseLogPrefix, request.Id);
        
        _logger.LogInformation("{UseCaseLogPrefix} Iniciando mapeamento dos novos dados para UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);
        usuario.Nome = request.Nome;
        usuario.Email = request.Email;
        _logger.LogInformation("{UseCaseLogPrefix} Mapeamento concluído para UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);
        
        _logger.LogInformation("{UseCaseLogPrefix} Iniciando persistência da atualização para UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);
        await _repositoryUser.UpdateUsuarioAsync(usuario);
        _logger.LogInformation("{UseCaseLogPrefix} Persistência concluída com sucesso para UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);
        
        _logger.LogInformation("{UseCaseLogPrefix} Processo de atualização finalizado com sucesso para UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);
        return Result.Ok();
    }
}