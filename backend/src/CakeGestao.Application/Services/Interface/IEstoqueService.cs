using CakeGestao.Application.Dtos.Requests.Estoque;
using CakeGestao.Application.Dtos.Responses;
using FluentResults;

namespace CakeGestao.Application.Services.Interface;

public interface IEstoqueService
{
    public Task<Result> CreateEstoqueAsync(CreateEstoqueRequest request);
    public Task<Result> AddQuantidadeEstoqueAsync(AddQuantidadeEstoqueRequest request);
    public Task<Result<List<ItemEstoqueResponse>>> GetAllItemEstoque(int empresaId);
}
