using CakeGestao.Application.Features.Auth.Common;
using CakeGestao.Application.Features.Auth.Refresh;
using CakeGestao.Domain.Interfaces.Repositories;
using CakeGestao.Domain.Interfaces.Security;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.Auth.Refresh;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<TokensResponse>>
{
    private readonly ILogger<RefreshTokenHandler> _logger;
    private readonly ITokenRepository _tokenRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private const string LogPrefix = "[Refresh Token]";

    public RefreshTokenHandler(ILogger<RefreshTokenHandler> logger, ITokenRepository tokenRepository, IUsuarioRepository usuarioRepository, IJwtTokenService jwtTokenService)
    {
        _logger = logger;
        _tokenRepository = tokenRepository;
        _usuarioRepository = usuarioRepository;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<TokensResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{UseCaseLogPrefix} Iniciando processo de refresh de token", LogPrefix);

        _logger.LogInformation("{UseCaseLogPrefix} Buscando refresh token no repositório", LogPrefix);
        var tokenResult = await _tokenRepository.GetRefreshTokenAsync(request.RefreshToken);
        if (tokenResult.IsFailed || tokenResult.Value.IsRevoked || tokenResult.Value.IsUsed || tokenResult.Value.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("{LogPrefix} Tentativa de refresh falhou. Token inválido, expirado ou reutilizado.", LogPrefix); ;
            return Result.Fail(new ValidationError(new List<string> { "Refresh token inválido ou expirado" }));
        };

        var tokenEntry = tokenResult.Value;
        tokenEntry.IsUsed = true;
        tokenEntry.IsRevoked = true;
        await _tokenRepository.UpdateRefreshTokenAsync(tokenEntry);
        _logger.LogInformation("{UseCaseLogPrefix} Refresh token marcado como usado. UsuarioId: {UsuarioId}", LogPrefix, tokenEntry.UsuarioId);

        var usuarioResult = await _usuarioRepository.GetByIdAsync(tokenEntry.UsuarioId);
        if (usuarioResult.IsFailed)
        {
            _logger.LogWarning("Usuario associado ao refresh token não encontrado");
            return Result.Fail(new NotFoundError("Usuario associado ao refresh token não encontrado"));
        }
        var usuario = usuarioResult.Value;
        _logger.LogInformation("Usuario associado ao refresh token encontrado com sucesso. UsuarioId: {UsuarioId}", usuario.Id);

        var tokens = await _jwtTokenService.TokenService(usuario.Id, usuario.Email, usuario.Role.ToString(), usuario.EmpresaId);
        _logger.LogInformation("{UseCaseLogPrefix} Novos tokens gerados com sucesso para o usuario Id: {Id}", LogPrefix, usuarioResult.Value.Id);

        var tokenResponse = new TokensResponse
        {
            AccessToken = tokens.accessToken,
            RefreshToken = tokens.refreshToken
        };
        _logger.LogInformation("{UseCaseLogPrefix} Mapeamento do RefreshTokenResponce realizado com sucesso para o usuario Id: {Id}", LogPrefix, usuarioResult.Value.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Finalizando o processo de refresh do token do usuario Id: {Id}", LogPrefix, usuarioResult.Value.Id);
        return Result.Ok(tokenResponse);
    }
}
