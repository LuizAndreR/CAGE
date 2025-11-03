using CakeGestao.Application.Dtos.Requests.Usuario;
using FluentResults;

namespace CakeGestao.Application.UseCases.User.Interface;

public interface IUpdateUserUseCase
{
    public Task<Result> ExecuteAsync(UpdateUsuarioRequest request, int id);
}