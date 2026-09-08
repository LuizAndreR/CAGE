using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Infrastructure.Data.Repositories;

public class TokenRepository : ITokenRepository
{
    private readonly CageContext _context;
    private readonly ILogger<TokenRepository> _logger;
    private const string LogPrefix = "[Token Repository]";

    public TokenRepository(CageContext context, ILogger<TokenRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<TokenRefresh>> GetRefreshTokenAsync(string refreshToken)
    {
        _logger.LogDebug("{LogPrefix} Consultando validade do Refresh Token.", LogPrefix);

        var token = await _context.TokensRefresh.AsNoTracking().FirstOrDefaultAsync(r => r.RefreshToken == refreshToken);
        if (token == null)
        {
            _logger.LogWarning("{LogPrefix} Token não encontrado ou inválido.", LogPrefix);
            return Result.Fail("Refresh token não encontrado.");
        }

        return Result.Ok(token);
    }

    public async Task<Result> SaveRefreshTokenAsync(TokenRefresh refreshToken)
    {
        await _context.TokensRefresh.AddAsync(refreshToken);
        await _context.SaveChangesAsync();

        _logger.LogInformation("{LogPrefix} Nova sessão registrada. UsuarioId: {UsuarioId} | Expira em: {Expiracao}", LogPrefix, refreshToken.UsuarioId, refreshToken.ExpiresAt);
        return Result.Ok();
    }

    public async Task<Result> UpdateRefreshTokenAsync(TokenRefresh refreshToken)
    {
        _context.TokensRefresh.Update(refreshToken);
        await _context.SaveChangesAsync();

        _logger.LogInformation("{LogPrefix} Token atualizado (Revogado/Usado). UsuarioId: {UsuarioId}", LogPrefix, refreshToken.UsuarioId);
        return Result.Ok();
    }
}
