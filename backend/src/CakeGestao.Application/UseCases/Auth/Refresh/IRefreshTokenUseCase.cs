using CakeGestao.Application.Dtos.Requests.Auth;
using CakeGestao.Application.Dtos.Responses;
using FluentResults;

namespace CakeGestao.Application.UseCases.Auth.Refresh;

public interface IRefreshTokenUseCase
{
    public Task<Result<TokensResponce>> Execute(RefreshTokenRequest request);
}
