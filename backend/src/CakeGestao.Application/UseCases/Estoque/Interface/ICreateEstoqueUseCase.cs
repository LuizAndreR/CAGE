using CakeGestao.Application.Dtos.Requests.Estoque;
using FluentResults;

namespace CakeGestao.Application.UseCases.Estoque.Interface;

public interface ICreateEstoqueUseCase
{
    public Task<Result> ExecuteAsync(CreateEstoqueRequest request);
}
