using CakeGestao.Application.Dtos.Requests.Estoque;
using FluentResults;

namespace CakeGestao.Application.UseCases.Estoque.Interface;

public interface IAddQuantidadeEstoqueUseCase
{
    public Task<Result> ExecuteAsync(AddQuantidadeEstoqueRequest request);
}
