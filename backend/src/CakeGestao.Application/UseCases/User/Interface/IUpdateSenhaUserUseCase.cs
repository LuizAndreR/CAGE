using CakeGestao.Application.Dtos.Requests.Usuario;
using FluentResults;

namespace CakeGestao.Application.UseCases.User.Interface;

public interface IUpdateSenhaUserUseCase
{
    public Task<Result> ExecuteAsync(UpdateSenhaUsuarioRequest request, int id);
}
