using CakeGestao.Application.Dtos.Requests.Usuario;
using CakeGestao.Application.Dtos.Responses;
using CakeGestao.Application.Services.Interface;
using CakeGestao.Application.UseCases.User.Interface;
using FluentResults;

namespace CakeGestao.Application.Services.Service;

public class UserService : IUserService
{
    private readonly IGetAllUsuarioUseCase _getAllUserUseCase;
    private readonly IGetUsuarioUseCase _getUsuarioUseCase;
    private readonly IUpdateUserUseCase _updateUserUseCase;
    private readonly IUpdateSenhaUsuarioUseCase _updateSenhaUsuarioUseCase;
    private readonly IDeleteUsuarioUseCase _deleteUsuarioUseCase;

    public UserService(IGetAllUsuarioUseCase getAllUserUseCase,IGetUsuarioUseCase getUsuarioUseCase, IUpdateUserUseCase updateUserUseCase, IUpdateSenhaUsuarioUseCase updateSenhaUsuarioUseCase, IDeleteUsuarioUseCase deleteUsuarioUseCase)
    {
        _getAllUserUseCase = getAllUserUseCase;
        _getUsuarioUseCase = getUsuarioUseCase;
        _updateUserUseCase = updateUserUseCase;
        _updateSenhaUsuarioUseCase = updateSenhaUsuarioUseCase;
        _deleteUsuarioUseCase = deleteUsuarioUseCase;
    }
    
    public async Task<Result<List<UsuarioResponse>>> GetAllUsuarioAsync() => await _getAllUserUseCase.Execute();
    public async Task<Result<UsuarioResponse>> GetUsuarioByIdAsync(int id) => await _getUsuarioUseCase.Execute(id);
    public async Task<Result> UpdateUsuarioAsync(UpdateUsuarioRequest request, int id) => await _updateUserUseCase.ExecuteAsync(request, id);
    public async Task<Result> UpdateSenhaUsuarioAsync(UpdateSenhaUsuarioRequest request, int id) => await _updateSenhaUsuarioUseCase.ExecuteAsync(request, id);
    public async Task<Result> DeleteUsuarioAsync(int usuarioId) => await _deleteUsuarioUseCase.ExecuteAsync(usuarioId); 
}
