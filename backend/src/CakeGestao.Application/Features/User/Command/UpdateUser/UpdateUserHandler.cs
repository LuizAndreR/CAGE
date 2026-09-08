using CakeGestao.Domain.Exceptions;
using CakeGestao.Domain.Interfaces.Repositories;
using CakeGestao.Infrastructure.Data.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.User.Command.UpdateUser;

public class UpdateUserHandler: IRequestHandler<UpdateUsuarioCommand, Result>
{
    private readonly IUsuarioRepository _usuarioRepository; 
    private readonly IValidator<UpdateUsuarioCommand> _validator;
    private readonly ILogger<UpdateUserHandler> _logger;
    private const string LogPrefix = "[Update Usuario Handler]";

    public UpdateUserHandler(IUsuarioRepository usuarioRepository, IValidator<UpdateUsuarioCommand> validator, ILogger<UpdateUserHandler> logger)
    {
        _usuarioRepository = usuarioRepository;
        _validator = validator;
        _logger = logger;
    }


    public async Task<Result> Handle(UpdateUsuarioCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Iniciando atualização de perfil. ID: {Id} | Novo Email: {Email}", LogPrefix, request.Id, request.Email);

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
            _logger.LogWarning("{LogPrefix} Usuário não encontrado. ID: {Id}", LogPrefix, request.Id);
            return Result.Fail(new NotFoundError("Usuario nao foi encontrado no banco de dados"));
        }
        var usuario = usuarioResult.Value;

        if (usuario.Email != request.Email)
        {
            var emailExists = await _usuarioRepository.GetUsuarioByEmailAsync(request.Email);
            if (emailExists.IsSuccess)
            {
                _logger.LogWarning("{LogPrefix} Email já está em uso. Email: {Email}", LogPrefix, request.Email);
                return Result.Fail(new ConflictError("Email ja esta em uso por outro usuario"));
            }
        }

        usuario.AtualizarUsuario(request.Nome, request.Email);

        await _usuarioRepository.UpdateUsuarioAsync(usuario);

        _logger.LogInformation("{LogPrefix} Perfil atualizado com sucesso. ID: {Id}", LogPrefix, request.Id);
        return Result.Ok();
    }
}