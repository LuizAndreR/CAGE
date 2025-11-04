using CakeGestao.Application.Dtos.Requests.Usuario;
using CakeGestao.Application.Dtos.Responses;
using FluentResults;

namespace CakeGestao.Application.Services.Interface;

public interface IUserService
{
    public Task<Result<UsuarioResponse>> GetUsuarioByIdAsync(int id);
    public Task<Result> UpdateUsuarioAsync(UpdateUsuarioRequest request, int id);
    public Task<Result> UpdateSenhaUsuarioAsync(UpdateSenhaUsuarioRequest request, int id);
}
