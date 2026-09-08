using CakeGestao.Domain.Entities;
using FluentResults;

namespace CakeGestao.Domain.Interfaces.Repositories;

public interface IEstoqueRepository
{
    public Task<int> GetTotalItensEstoqueAsync(int empresaId, CancellationToken cancellationToken);
    public Task<bool> ExisteDependenciaComReceitaAsync(int itemId, int empresaId);
    public Task<Result<Estoque>> GetItemEstoqueByIdAsync(int id, int? empresaId);
    public Task<Result<List<Estoque>>> GetItensByIdsAsync(IEnumerable<int> ids, int empresaId);
    public Task<Result<List<Estoque>>> GetAllItemEstoqueByEmpresaIdAsync(int empresaId);
    public Task<Result<List<Estoque>>> GetAlertaEstoqueByEmpresaIdAsync(int empresaId, int quantidadeMinima);
    public Task<Result> ExistItemByNome(string nome, int empresaId, string marca);
    public Task CreateItemEstoqueAsync(Estoque itemEstoque);
    public Task UpdateItemEstoqueAsync(Estoque itemEstoque);
    public Task DeleteItemEstoqueAsync(Estoque itemEstoque);
}
