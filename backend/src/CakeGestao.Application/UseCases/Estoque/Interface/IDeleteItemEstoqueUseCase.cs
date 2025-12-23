using CakeGestao.Application.Dtos.Requests.Estoque;
using FluentResults;

namespace CakeGestao.Application.UseCases.Estoque.Interface;

public interface IDeleteItemEstoqueUseCase
{
    public Task<Result> ExecuteAsync(ItemEstoqueRequest request);
}
