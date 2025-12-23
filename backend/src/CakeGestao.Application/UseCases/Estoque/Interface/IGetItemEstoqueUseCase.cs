using CakeGestao.Application.Dtos.Requests.Estoque;
using CakeGestao.Application.Dtos.Responses;
using FluentResults;

namespace CakeGestao.Application.UseCases.Estoque.Interface;

public interface IGetItemEstoqueUseCase
{
    public Task<Result<ItemEstoqueResponse>> ExecuteAsync(ItemEstoqueRequest request);
}
