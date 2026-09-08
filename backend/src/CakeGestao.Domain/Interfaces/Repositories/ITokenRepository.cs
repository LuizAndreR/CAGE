using CakeGestao.Domain.Entities;
using FluentResults;

namespace CakeGestao.Domain.Interfaces.Repositories;

public interface ITokenRepository
{
    public Task<Result> SaveRefreshTokenAsync(TokenRefresh refreshToken);
    public Task<Result<TokenRefresh>> GetRefreshTokenAsync(string refreshToken);
    public Task<Result> UpdateRefreshTokenAsync(TokenRefresh refreshToken);
}
