using CakeGestao.Application.Dtos.Responses;
using FluentResults;

namespace CakeGestao.Application.Services.Interface;

public interface IUserService
{
    public Task<Result<UsuarioResponse>> GetUsuarioByIdAsync(int id);
}
