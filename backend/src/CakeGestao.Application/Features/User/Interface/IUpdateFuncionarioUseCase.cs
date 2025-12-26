using CakeGestao.Application.Dtos.Requests.Usuario;
using FluentResults;

namespace CakeGestao.Application.UseCases.User.Interface;

public interface IUpdateFuncionarioUseCase
{
    public Task<Result> ExecuteAsync(UpdateFuncionarioUsuarioRequest request);
}
