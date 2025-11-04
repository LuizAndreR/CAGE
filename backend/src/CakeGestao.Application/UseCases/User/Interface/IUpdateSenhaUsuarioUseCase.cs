using CakeGestao.Application.Dtos.Requests.Usuario;
using FluentResults;

namespace CakeGestao.Application.UseCases.User.Interface;

public interface IUpdateSenhaUsuarioUseCase
{
    public Task<Result> ExecuteAsync(UpdateSenhaUsuarioRequest request, int id);
}
