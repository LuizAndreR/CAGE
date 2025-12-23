using CakeGestao.Application.Dtos.Requests.Estoque;
using FluentResults;

namespace CakeGestao.Application.UseCases.Estoque.Interface;

public interface IUpdateItemEstoqueUseCase
{
    public Task<Result> ExecuteAsync(UpdateItemEstoqueRequest request);
}
