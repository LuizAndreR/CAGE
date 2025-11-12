using CakeGestao.Application.Dtos.Requests.Auth;
using CakeGestao.Application.Dtos.Responses;
using CakeGestao.Domain.Interfaces.Repositories;
using CakeGestao.Domain.Interfaces.Security;
using FluentResults;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.Auth.Refresh;

public class RefreshTokenUseCase : IRefreshTokenUseCase
{
    private readonly ILogger<RefreshTokenUseCase> _logger;
    private readonly ITokenRepository _tokenRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public RefreshTokenUseCase(ILogger<RefreshTokenUseCase> logger, ITokenRepository tokenRepository, IUsuarioRepository usuarioRepository, IJwtTokenService jwtTokenService)
    {
        _logger = logger;
        _tokenRepository = tokenRepository;
        _usuarioRepository = usuarioRepository;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<TokensResponce>> Execute(RefreshTokenRequest request)
    {
        _logger.LogInformation("Iniciando o processo refrash do token");

        var tokenResult = await _tokenRepository.GetRefreshTokenAsync(request.RefreshToken);
        _logger.LogInformation("Busca do refresh token realizado com susseso");

        _logger.LogInformation("Validando refresh token");
        if (tokenResult.IsFailed || tokenResult.Value.IsRevoked || tokenResult.Value.IsUsed || tokenResult.Value.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Refresh token inválido ou expirado");
            return Result.Fail(new ValidationError(new List<string> { "Refresh token inválido ou expirado" }));
        }
        _logger.LogInformation("Refresh token validado com sucesso");

        _logger.LogInformation("Atualizando o status do refresh token para usado");
        var tokenEntry = tokenResult.Value;
        tokenEntry.IsUsed = true;
        tokenEntry.IsRevoked = true;
        await _tokenRepository.UpdateRefreshTokenAsync(tokenEntry);
        _logger.LogInformation("Atatus do refresh token atulizado para usado");

        _logger.LogInformation("Buscando o usuario associado ao refresh token");
        var usuarioResult = await _usuarioRepository.GetByIdAsync(tokenEntry.UsuarioId);
        if (usuarioResult.IsFailed)
        {
            _logger.LogWarning("Usuario associado ao refresh token não encontrado");
            return Result.Fail(new NotFoundError("Usuario associado ao refresh token não encontrado"));
        }
        _logger.LogInformation("Busca do usuario associado ao refresh token realizada com sucesso. Id usuario: {Id}", usuarioResult.Value.Id);

        _logger.LogInformation("Gerando novos tokens de acesso e refresh token para o usuario Id: {Id}", usuarioResult.Value.Id);
        var tokens = await _jwtTokenService.TokenService(usuarioResult.Value.Id, usuarioResult.Value.Email, usuarioResult.Value.Role.ToString());
        _logger.LogInformation("Novos tokens gerados com sucesso para o usuario Id: {Id}", usuarioResult.Value.Id);

        _logger.LogInformation("Mapeando RefreshTokenResponce com os novos tokens do usuario Id: {Id}", usuarioResult.Value.Id);
        var tokenResponse = new TokensResponce
        {
            AccessToken = tokens.accessToken,
            RefreshToken = tokens.refreshToken
        };
        _logger.LogInformation("Mapeamento do RefreshTokenResponce realizado com sucesso para o usuario Id: {Id}", usuarioResult.Value.Id);

        _logger.LogInformation("Finalizando o processo de refresh do token do usuario Id: {Id}", usuarioResult.Value.Id);
        return Result.Ok(tokenResponse);
    }
}
