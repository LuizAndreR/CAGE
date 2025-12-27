using System.Linq;
using CakeGestao.Application.Features.Auth.Cadastro;
using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Enun;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.Auth.Cadastro;

public class CadastroHandler : IRequestHandler<CadastroCommand, Result>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ILogger<CadastroHandler> _logger;
    private readonly IValidator<CadastroCommand> _validator;    
    private const string UseCaseLogPrefix = "[Cadastro Usuario]";

    public CadastroHandler(IUsuarioRepository usuarioRepository, ILogger<CadastroHandler> logger, IValidator<CadastroCommand> validator)
    {
        _usuarioRepository = usuarioRepository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result> Handle(CadastroCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{UseCaseLogPrefix} Iniciando processo de cadastro de usuário. Email: {Email}", UseCaseLogPrefix, request.Email);

        _logger.LogInformation("{UseCaseLogPrefix} Validando dados para cadastro de usuário. Email: {Email}", UseCaseLogPrefix, request.Email);
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var listErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("{UseCaseLogPrefix} Validação falhou durante cadastro. Email: {Email}. Erros: {Errors}", UseCaseLogPrefix, request.Email, listErrors);
            return Result.Fail(new ValidationError(listErrors));
        }

        _logger.LogInformation("{UseCaseLogPrefix} Verificando existência de usuário com email fornecido. Email: {Email}", UseCaseLogPrefix, request.Email);
        var usuarioExistenteResult = await _usuarioRepository.GetUsuarioByEmailAsync(request.Email);
        if (usuarioExistenteResult.IsSuccess)
        {
            _logger.LogWarning("{UseCaseLogPrefix} Tentativa de cadastro para email já existente. Email: {Email}", UseCaseLogPrefix, request.Email);
            return Result.Fail(new ConflictError("Usuário com mesmo email já existe"));
        }

        _logger.LogInformation("{UseCaseLogPrefix} Criando entidade de usuário para persistência. Email: {Email}", UseCaseLogPrefix, request.Email);
        var dataCriacao = DateTime.UtcNow;
        var userRole = Enum.Parse<UserRole>(request.Role, true);
        var senhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha);
        int? empresaIdTratado = request.EmpresaId > 0 ? request.EmpresaId : null;
        Usuario usuario = new Usuario(
            request.Nome,
            request.Email,
            senhaHash,
            userRole,
            dataCriacao,
            empresaIdTratado
        );

        _logger.LogInformation("{UseCaseLogPrefix} Persistindo novo usuário. Email: {Email}", UseCaseLogPrefix, request.Email);
        await _usuarioRepository.CreateUserAsync(usuario);

        _logger.LogInformation("{UseCaseLogPrefix} Cadastro concluído com sucesso. Email: {Email}, UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Email, usuario.Id);
        return Result.Ok();
    }
}
