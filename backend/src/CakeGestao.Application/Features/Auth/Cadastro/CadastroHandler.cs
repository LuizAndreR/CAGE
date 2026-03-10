using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Auth.Cadastro;

public class CadastroHandler : IRequestHandler<CadastroCommand, Result>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ILogger<CadastroHandler> _logger;
    private readonly IValidator<CadastroCommand> _validator;    
    private const string LogPrefix = "[Cadastro Handler]";

    public CadastroHandler(IUsuarioRepository usuarioRepository, ILogger<CadastroHandler> logger, IValidator<CadastroCommand> validator)
    {
        _usuarioRepository = usuarioRepository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result> Handle(CadastroCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Iniciando processo de cadastro. Email: {Email} | Role: {Role}", LogPrefix, request.Email, request.Role);

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var listErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("{LogPrefix} Dados inválidos no cadastro. Email: {Email}. Erros: {Errors}", LogPrefix, request.Email, string.Join(", ", listErrors));
            return Result.Fail(new ValidationError(listErrors));
        }

        var userRole = Enum.Parse<UserRole>(request.Role, true);
        if (userRole == UserRole.Admin && request.AdminRole == false)
        {
            _logger.LogWarning("{LogPrefix} Conflito: Somente Admin pode cadastra novo usuario role Admin", LogPrefix);
            return Result.Fail(new ConflictError("Somente Admin pode cadastra novo usuario role Admin"));
        }

        var usuarioExistenteResult = await _usuarioRepository.GetUsuarioByEmailAsync(request.Email);
        if (usuarioExistenteResult.IsSuccess)
        {
            _logger.LogWarning("{LogPrefix} Conflito: Email já cadastrado. Email: {Email}", LogPrefix, request.Email);
            return Result.Fail(new ConflictError("Usuário com mesmo email já existe"));
        }

        var dataCriacao = DateTime.UtcNow;
        
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

        await _usuarioRepository.CreateUserAsync(usuario);

        _logger.LogInformation("{LogPrefix} Usuário cadastrado com sucesso. ID: {Id} | Email: {Email}", LogPrefix, usuario.Id, request.Email);
        return Result.Ok();
    }
}
