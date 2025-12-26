using CakeGestao.Application.Dtos.Responses;
using FluentResults;

namespace CakeGestao.Application.UseCases.User.Interface;

public interface IGetAllUsuarioUseCase
{
    public Task<Result<List<UsuarioResponse>>> Execute();
}