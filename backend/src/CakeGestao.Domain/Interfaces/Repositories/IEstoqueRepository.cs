using CakeGestao.Domain.Entities;
using FluentResults;

namespace CakeGestao.Domain.Interfaces.Repositories;

public interface IEstoqueRepository
{
    public Task<Result<ItemEstoque>> GetItemEstoqueByIdAsync(int id);
    public Task<Result<List<ItemEstoque>>> GetAllItemEstoqueByEmpresaIdAsync(int empresaId);
    public Task<Result<List<ItemEstoque>>> GetAlertaEstoqueByEmpresaIdAsync(int empresaId, int QuantidadeMinima);
    public Task<Result> ExistItemByNome(string nome, int empresaId);
    public Task CreateItemEstoqueAsync(ItemEstoque itemEstoque);
    public Task UpdateItemEstoqueAsync(ItemEstoque itemEstoque);
    public Task DeleteItemEstoqueAsync(ItemEstoque itemEstoque);
}
