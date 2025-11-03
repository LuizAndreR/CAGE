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

    public TokenRepository(CageContext context, ILogger<TokenRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<TokenRefresh>> GetRefreshTokenAsync(string refreshToken)
    {
        _logger.LogInformation("Buscando refresh token no banco de dados.");

        var token = await _context.TokensRefresh.FirstOrDefaultAsync(r => r.RefreshToken == refreshToken);
        if (token == null)
        {
            _logger.LogInformation("Refresh token não encontrado no banco de dados.");
            return Result.Fail("Refresh token não encontrado.");
        }

        _logger.LogInformation("Refresh token encontrado no banco de dados.");
        return Result.Ok(token);
    }

    public async Task<Result> SaveRefreshTokenAsync(TokenRefresh refreshToken)
    {
        _logger.LogInformation("Salvando refresh token no banco de dados para o usuário: {UsuarioId}", refreshToken.UsuarioId);

        _context.TokensRefresh.Add(refreshToken);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Refresh token salvo com sucesso no banco de dados para o usuário: {UsuarioId}", refreshToken.UsuarioId);
        return Result.Ok();
    }

    public async Task<Result> UpdateRefreshTokenAsync(TokenRefresh refreshToken)
    {
        _logger.LogInformation("Atualizando refresh token no banco de dados para o usuário: {UsuarioId}", refreshToken.UsuarioId);

        _context.TokensRefresh.Update(refreshToken);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Refresh token atualizado com sucesso no banco de dados para o usuário: {UsuarioId}", refreshToken.UsuarioId);
        return Result.Ok();
    }
}
