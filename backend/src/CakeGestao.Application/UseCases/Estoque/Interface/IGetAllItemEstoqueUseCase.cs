using CakeGestao.Application.Dtos.Responses;
using FluentResults;

namespace CakeGestao.Application.UseCases.Estoque.Interface;

public interface IGetAllItemEstoqueUseCase
{
    public Task<Result<List<ItemEstoqueResponse>>> ExecuteAsync(int empresaId);
}
