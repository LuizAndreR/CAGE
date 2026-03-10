using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.User.Command.Delete;

public class DeleteUsuarioHandler : IRequestHandler<DeleteUsuarioCommand, Result>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ILogger<DeleteUsuarioHandler> _logger;
    private readonly IValidator<DeleteUsuarioCommand> _validator;
    private const string LogPrefix = "[Delete User Handler]";

    public DeleteUsuarioHandler(IUsuarioRepository usuarioRepository, ILogger<DeleteUsuarioHandler> logger, IValidator<DeleteUsuarioCommand> validator)
    {
        _usuarioRepository = usuarioRepository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result> Handle(DeleteUsuarioCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Solicitada exclusão de usuário. ID Alvo: {Id}", LogPrefix, request.Id);

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("{LogPrefix} Dados inválidos. EmpresaId: {EmpresaId}. Erros: {Errors}", LogPrefix, request.EmpresaId, string.Join(", ", errors));
            return Result.Fail(new ValidationError(errors));
        }

        var usuarioResult = await _usuarioRepository.GetByIdAsync(request.Id, request.EmpresaId);
        if (usuarioResult.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Usuário não encontrado para exclusão. ID: {Id}", LogPrefix, request.Id);
            return Result.Fail(new NotFoundError("Usuário não encontrado."));
        }

        await _usuarioRepository.DeleteAsync(usuarioResult.Value);

        _logger.LogInformation("{LogPrefix} Usuário excluído permanentemente. ID: {Id}", LogPrefix, request.Id);
        return Result.Ok();
    }
}
