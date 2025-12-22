using CakeGestao.Application.Dtos.Requests.Estoque;
using FluentResults;

namespace CakeGestao.Application.Services.Interface;

public interface IEstoqueService
{
    public Task<Result> CreateEstoqueAsync(CreateEstoqueRequest request);
    public Task<Result> AddQuantidadeEstoqueAsync(AddQuantidadeEstoqueRequest request);
}
