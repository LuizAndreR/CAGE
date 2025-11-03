using CakeGestao.Application.Dtos.Requests.Usuario;
using CakeGestao.Application.Dtos.Responses;
using CakeGestao.Application.Services.Interface;
using CakeGestao.Application.UseCases.User.Interface;
using FluentResults;

namespace CakeGestao.Application.Services.Service;

public class UserService : IUserService
{
    private readonly IGetUsuarioUseCase _getUsuarioUseCase;
    private readonly IUpdateUserUseCase _updateUserUseCase;
    
    public UserService(IGetUsuarioUseCase getUsuarioUseCase, IUpdateUserUseCase updateUserUseCase)
    {
        _getUsuarioUseCase = getUsuarioUseCase;
        _updateUserUseCase = updateUserUseCase;
    }

    public async Task<Result<UsuarioResponse>> GetUsuarioByIdAsync(int id) => await _getUsuarioUseCase.Execute(id);
    public async Task<Result> UpdateUsuarioAsync(UpdateUsuarioRequest request, int id) => await _updateUserUseCase.ExecuteAsync(request, id);
}
