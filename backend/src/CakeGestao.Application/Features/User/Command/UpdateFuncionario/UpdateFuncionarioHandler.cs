using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.User.Command.UpdateFuncionario;

public class UpdateFuncionarioHandler : IRequestHandler<UpdateFuncionarioCommand, Result>
{
    private readonly ILogger<UpdateFuncionarioHandler> _logger;   
    private readonly IUsuarioRepository _repositoryUser;
    private readonly IValidator<UpdateFuncionarioCommand> _validator;
    private const string UseCaseLogPrefix = "[Update Funcionario]";

    public UpdateFuncionarioHandler(ILogger<UpdateFuncionarioHandler> logger, IUsuarioRepository repositoryUser, IValidator<UpdateFuncionarioCommand> validator)
    {
        _logger = logger;
        _repositoryUser = repositoryUser;
        _validator = validator;
    }

    public async Task<Result> Handle(UpdateFuncionarioCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{UseCaseLogPrefix} Iniciando processo de atualização de funcionário. UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Validando requisição para UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
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

        _logger.LogInformation("{UseCaseLogPrefix} Iniciando mapeamento das alterações para UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);
        var role = Enum.Parse<UserRole>(request.Role);
        usuario.AtualizarFuncionario(request.Nome, role);
        _logger.LogInformation("{UseCaseLogPrefix} Mapeamento concluído para UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Iniciando persistência das alterações para UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);
        await _repositoryUser.UpdateUsuarioAsync(usuario);
        _logger.LogInformation("{UseCaseLogPrefix} Persistência concluída com sucesso para UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Processo de atualização finalizado com sucesso para UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);
        return Result.Ok();
    }
}
