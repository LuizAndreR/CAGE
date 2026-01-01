using CakeGestao.Application.Features.Auth.Common;
using CakeGestao.Domain.Interfaces.Repositories;
using CakeGestao.Domain.Interfaces.Security;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Auth.Login;

public class LoginHandler : IRequestHandler<LoginCommand, Result<TokensResponse>>
{ 
    private readonly ILogger<LoginHandler> _logger;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IValidator<LoginCommand> _validator;
    private const string UseCaseLogPrefix = "[Login]";

    public LoginHandler(ILogger<LoginHandler> logger, IJwtTokenService jwtTokenService, IUsuarioRepository usuarioRepository, IValidator<LoginCommand> validator)
    {
        _logger = logger;
        _jwtTokenService = jwtTokenService;
        _usuarioRepository = usuarioRepository;
        _validator = validator;
    }

    public async Task<Result<TokensResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{UseCaseLogPrefix} Iniciando processo de login. Email: {Email}", UseCaseLogPrefix, request.Email);

        _logger.LogInformation("{UseCaseLogPrefix} Validando dados de login", UseCaseLogPrefix);
        ValidationResult validationResult = _validator.Validate(request);
        if (!validationResult.IsValid)
        {
            var listErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("{UseCaseLogPrefix} Validação de login falhou. Email: {Email}. Erros: {Errors}", UseCaseLogPrefix, request.Email, listErrors);
            return Result.Fail(new ValidationError(listErrors));
        }

        _logger.LogInformation("{UseCaseLogPrefix} Recuperando usuário por email", UseCaseLogPrefix);
        var usuarioResult = await _usuarioRepository.GetUsuarioByEmailAsync(request.Email);
        if (usuarioResult.IsFailed)
        {
            _logger.LogWarning("{UseCaseLogPrefix} Usuário não encontrado ou erro na busca. Email: {Email}", UseCaseLogPrefix, request.Email);
            return Result.Fail(new ValidationError(new List<string> { "Email ou senha inválidos" }));
        }

        var usuario = usuarioResult.Value;

        if (!BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
        {
            _logger.LogWarning("{UseCaseLogPrefix} Senha inválida para o usuário. Email: {Email}", UseCaseLogPrefix, request.Email);
            return Result.Fail(new ValidationError(new List<string> { "Email ou senha inválidos" }));
        }

        _logger.LogInformation("{UseCaseLogPrefix} Gerando tokens de autenticação para UsuarioId: {UsuarioId}", UseCaseLogPrefix, usuario.Id);
        var tokens = await _jwtTokenService.TokenService(usuario.Id, usuario.Email, usuario.Role.ToString(), usuario.EmpresaId);

        _logger.LogInformation("{UseCaseLogPrefix} Atualizando último login do usuário. UsuarioId: {UsuarioId}", UseCaseLogPrefix, usuario.Id);
        usuario.AtualizarUltimoLogin(DateTime.UtcNow);
        await _usuarioRepository.UpdateUsuarioAsync(usuario);

        var tokenResponse = new TokensResponse
        {
            AccessToken = tokens.accessToken,
            RefreshToken = tokens.refreshToken
        };

        _logger.LogInformation("{UseCaseLogPrefix} Login concluído com sucesso. UsuarioId: {UsuarioId}", UseCaseLogPrefix, usuario.Id);
        return Result.Ok(tokenResponse);
    }
}
