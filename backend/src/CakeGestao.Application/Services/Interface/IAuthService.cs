using CakeGestao.Application.Dtos.Requests.Auth;
using CakeGestao.Application.Dtos.Responses;
using FluentResults;

namespace CakeGestao.Application.Services.Interface;

public interface IAuthService
{
    public Task<Result> CreateUserAsync(CadastroRequest request);
    public Task<Result<TokensResponce>> RefreshToken(RefreshTokenRequest request);
    public Task<Result<TokensResponce>> Login(LoginRequest request);
}
