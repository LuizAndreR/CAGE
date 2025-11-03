using CakeGestao.Application.Dtos.Responses;
using FluentResults;

namespace CakeGestao.Application.UseCases.Auth.Refresh;

public interface IRefreshTokenUseCase
{
    public Task<Result<RefreshTokenResponce>> Execute(string refreshToken);
}
