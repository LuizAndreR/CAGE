using CakeGestao.Application.Features.Auth.Common;
using CakeGestao.Domain.Exceptions;
using CakeGestao.Domain.Interfaces.Repositories;
using CakeGestao.Domain.Interfaces.Security;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Auth.Refresh;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<TokensResponse>>
{
    private readonly ILogger<RefreshTokenHandler> _logger;
    private readonly ITokenRepository _tokenRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private const string LogPrefix = "[Refresh Token Handler]";

    public RefreshTokenHandler(ILogger<RefreshTokenHandler> logger, ITokenRepository tokenRepository, IUsuarioRepository usuarioRepository, IJwtTokenService jwtTokenService)
    {
        _logger = logger;
        _tokenRepository = tokenRepository;
        _usuarioRepository = usuarioRepository;
        _jwtTokenService = jwtTokenService;
    }
    
    // O token já foi usado, isso pode indicar roubo de token (Replay Attack)
    // O ideal seria invalidar todos os tokens desse usuário, mas por hora vamos apenas negar.
    //Fazer depois
    public async Task<Result<TokensResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Iniciando renovação de sessão (Refresh Token).", LogPrefix);

        var tokenResult = await _tokenRepository.GetRefreshTokenAsync(request.RefreshToken);
        if (tokenResult.IsFailed || tokenResult.Value.IsRevoked || tokenResult.Value.IsUsed || tokenResult.Value.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("{LogPrefix} Falha na renovação: Token inválido, expirado ou reutilizado.", LogPrefix);
            return Result.Fail(new ValidationError(new List<string> { "Refresh token inválido ou expirado" }));
        };

        var tokenEntry = tokenResult.Value;
        tokenEntry.IsUsed = true;
        tokenEntry.IsRevoked = true;
        await _tokenRepository.UpdateRefreshTokenAsync(tokenEntry);

        var usuarioResult = await _usuarioRepository.GetByIdAsync(tokenEntry.UsuarioId, null);
        if (usuarioResult.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Usuário vinculado ao token não encontrado. ID: {UsuarioId}", LogPrefix, tokenEntry.UsuarioId);
            return Result.Fail(new NotFoundError("Usuario associado ao refresh token não encontrado"));
        }
        var usuario = usuarioResult.Value;
        var tokens = await _jwtTokenService.TokenService(usuario.Id, usuario.Nome, usuario.Email, usuario.Role.ToString(), usuario.EmpresaId);
        var tokenResponse = new TokensResponse
        {
            AccessToken = tokens.accessToken,
            RefreshToken = tokens.refreshToken
        };

        _logger.LogInformation("{LogPrefix} Sessão renovada com sucesso. UsuarioId: {UsuarioId}", LogPrefix, usuario.Id);
        return Result.Ok(tokenResponse);
    }
}
