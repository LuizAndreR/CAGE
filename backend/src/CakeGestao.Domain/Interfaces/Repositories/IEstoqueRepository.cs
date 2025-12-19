using CakeGestao.Domain.Entities;
using FluentResults;

namespace CakeGestao.Domain.Interfaces.Repositories;

public interface IEstoqueRepository
{
    public Task<Result> ExistItemByNome(string nome);
    public Task CreateItemEstoqueAsync(ItemEstoque itemEstoque);
    public Task UpdateItemEstoqueAsync(ItemEstoque itemEstoque);
}
