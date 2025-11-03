using CakeGestao.Application.Dtos.Requests.Auth;
using CakeGestao.Application.Dtos.Responses;
using CakeGestao.Application.Services.Interface;
using CakeGestao.Application.UseCases.Auth.Cadastro;
using CakeGestao.Application.UseCases.Auth.Refresh;
using FluentResults;

namespace CakeGestao.Application.Services.Service;

public class AuthService : IAuthService
{
    private readonly ICadastroUseCase _cadastroUseCase;
    private readonly IRefreshTokenUseCase _refreshTokenUseCase;

    public AuthService(ICadastroUseCase cadastroUseCase, IRefreshTokenUseCase refreshTokenUseCase)
    {
        _cadastroUseCase = cadastroUseCase;
        _refreshTokenUseCase = refreshTokenUseCase;
    }

    public async Task<Result> CreateUserAsync(CadastroRequest request) => await _cadastroUseCase.Execute(request);
    public async Task<Result<RefreshTokenResponce>> RefreshToken(string refreshToken) => await _refreshTokenUseCase.Execute(refreshToken);
}
