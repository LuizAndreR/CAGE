using FluentResults;

namespace CakeGestao.Application.UseCases.User.Interface;

public interface IDeleteUsuarioUseCase
{
    public Task<Result> ExecuteAsync(int usuarioId);
}
