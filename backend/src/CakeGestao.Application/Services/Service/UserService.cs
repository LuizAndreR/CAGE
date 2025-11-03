using CakeGestao.Application.Dtos.Responses;
using CakeGestao.Application.Services.Interface;
using CakeGestao.Application.UseCases.User.Interface;
using FluentResults;

namespace CakeGestao.Application.Services.Service;

public class UserService : IUserService
{
    private readonly IGetUsuarioUseCase _getUsuarioUseCase;
    public UserService(IGetUsuarioUseCase getUsuarioUseCase)
    {
        _getUsuarioUseCase = getUsuarioUseCase;
    }

    public async Task<Result<UsuarioResponse>> GetUsuarioByIdAsync(int id) => await _getUsuarioUseCase.Execute(id);
}
