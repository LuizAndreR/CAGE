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
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IValidator<UpdateFuncionarioCommand> _validator;
    private const string LogPrefix = "[Update Funcionario Handler]";

    public UpdateFuncionarioHandler(ILogger<UpdateFuncionarioHandler> logger, IUsuarioRepository usuarioRepository, IValidator<UpdateFuncionarioCommand> validator)
    {
        _logger = logger;
        _usuarioRepository = usuarioRepository;
        _validator = validator;
    }

    public async Task<Result> Handle(UpdateFuncionarioCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Iniciando atualização de funcionário. ID: {Id} | Nova Role: {Role}", LogPrefix, request.Id, request.Role);

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("{LogPrefix} Dados inválidos. ID: {Id}. Erros: {Errors}", LogPrefix, request.Id, string.Join(", ", errors));
            return Result.Fail(new ValidationError(errors));
        }

        var usuarioResult = await _usuarioRepository.GetByIdAsync(request.Id, request.EmpresaId);
        if (usuarioResult.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Usuário não encontrado para atualização. ID: {Id}", LogPrefix, request.Id);
            return Result.Fail(new NotFoundError("Usuario nao foi encontrado no banco de dados"));
        }
        var usuario = usuarioResult.Value;

        var role = Enum.Parse<UserRole>(request.Role);
        usuario.AtualizarFuncionario(request.Nome, role);

        await _usuarioRepository.UpdateUsuarioAsync(usuario);

        _logger.LogInformation("{LogPrefix} Funcionário atualizado com sucesso. ID: {Id} | Novo Nome: {Nome} | Nova Role: {Role}", LogPrefix, request.Id, request.Nome, request.Role);
        return Result.Ok();
    }
}
