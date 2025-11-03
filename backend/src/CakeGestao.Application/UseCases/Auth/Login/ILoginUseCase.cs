using CakeGestao.Application.Dtos.Requests.Auth;
using CakeGestao.Application.Dtos.Responses;
using FluentResults;

namespace CakeGestao.Application.UseCases.Auth.Login;

public interface ILoginUseCase
{
    public Task<Result<TokensResponce>> Execute(LoginRequest request);
}
