using CakeGestao.Application.Dtos.Requests.Auth;
using CakeGestao.Application.Dtos.Responses;
using CakeGestao.Application.Services.Interface;
using CakeGestao.Application.UseCases.Auth.Cadastro;
using CakeGestao.Application.UseCases.Auth.Login;
using CakeGestao.Application.UseCases.Auth.Refresh;
using FluentResults;

namespace CakeGestao.Application.Services.Service;

public class AuthService : IAuthService
{
    private readonly ICadastroUseCase _cadastroUseCase;
    private readonly IRefreshTokenUseCase _refreshTokenUseCase;
    private readonly ILoginUseCase _loginUseCase;

    public AuthService(ICadastroUseCase cadastroUseCase, IRefreshTokenUseCase refreshTokenUseCase, ILoginUseCase loginUseCase)
    {
        _cadastroUseCase = cadastroUseCase;
        _refreshTokenUseCase = refreshTokenUseCase;
        _loginUseCase = loginUseCase;
    }

    public async Task<Result> CreateUserAsync(CadastroRequest request, int? empresaId) => await _cadastroUseCase.Execute(request, empresaId);
    public async Task<Result<TokensResponce>> RefreshToken(RefreshTokenRequest request) => await _refreshTokenUseCase.Execute(request);
    public async Task<Result<TokensResponce>> Login(LoginRequest request) => await _loginUseCase.Execute(request);  
}
