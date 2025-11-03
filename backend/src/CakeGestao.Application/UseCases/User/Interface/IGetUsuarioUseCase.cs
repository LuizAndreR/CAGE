using CakeGestao.Application.Dtos.Responses;
using FluentResults;

namespace CakeGestao.Application.UseCases.User.Interface;

public interface IGetUsuarioUseCase
{
    public Task<Result<UsuarioResponse>> Execute(int id);
}
