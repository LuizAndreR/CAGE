using CakeGestao.Application.Features.Auth.Common;
using CakeGestao.Domain.Exceptions;
using CakeGestao.Domain.Interfaces.Repositories;
using CakeGestao.Domain.Interfaces.Security;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Auth.Login;

public class LoginHandler : IRequestHandler<LoginCommand, Result<TokensResponse>>
{ 
    private readonly ILogger<LoginHandler> _logger;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IValidator<LoginCommand> _validator;
    private const string LogPrefix = "[Login Handler]";

    public LoginHandler(ILogger<LoginHandler> logger, IJwtTokenService jwtTokenService, IUsuarioRepository usuarioRepository, IValidator<LoginCommand> validator)
    {
        _logger = logger;
        _jwtTokenService = jwtTokenService;
        _usuarioRepository = usuarioRepository;
        _validator = validator;
    }

    public async Task<Result<TokensResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Tentativa de autenticação. Email: {Email}", LogPrefix, request.Email);

        var validationResult = _validator.Validate(request);
        if (!validationResult.IsValid)
        {
            var listErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("{LogPrefix} Dados de login inválidos (Formato). Email: {Email}. Erros: {Errors}", LogPrefix, request.Email, string.Join(", ", listErrors));
            return Result.Fail(new ValidationError(listErrors));
        }

        var usuarioResult = await _usuarioRepository.GetUsuarioByEmailAsync(request.Email);
        if (usuarioResult.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Falha de autenticação: Usuário não encontrado. Email: {Email}", LogPrefix, request.Email);
            return Result.Fail(new ValidationError(new List<string> { "Email ou senha inválidos" }));
        }
        var usuario = usuarioResult.Value;
        if (!BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
        {
            _logger.LogWarning("{LogPrefix} Falha de autenticação: Senha incorreta. Email: {Email}", LogPrefix, request.Email);
            return Result.Fail(new ValidationError(new List<string> { "Email ou senha inválidos" }));
        }

        var tokens = await _jwtTokenService.TokenService(usuario.Id, usuario.Email, usuario.Role.ToString(), usuario.EmpresaId);
        usuario.AtualizarUltimoLogin(DateTime.UtcNow);
        await _usuarioRepository.UpdateUsuarioAsync(usuario);

        var tokenResponse = new TokensResponse
        {
            AccessToken = tokens.accessToken,
            RefreshToken = tokens.refreshToken
        };

        _logger.LogInformation("{LogPrefix} Login realizado com sucesso. UsuarioId: {UsuarioId} | Role: {Role}", LogPrefix, usuario.Id, usuario.Role);
        return Result.Ok(tokenResponse);
    }
}
