using CakeGestao.Application.Dtos.Requests.Estoque;
using CakeGestao.Application.Dtos.Responses;
using FluentResults;

namespace CakeGestao.Application.UseCases.Estoque.Interface;

public interface IGetAlertaEstoqueUseCase
{
    public Task<Result<List<ItemEstoqueResponse>>> ExecuteAsync(ItemEstoqueRequest request);
}
