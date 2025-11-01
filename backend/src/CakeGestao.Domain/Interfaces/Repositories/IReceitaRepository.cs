using CakeGestao.Domain.Entities;
using FluentResults;

namespace CakeGestao.Domain.Interfaces.Repositories;

public interface IReceitaRepository
{
    public Task<Result<List<Receita>>> GetAllReceitasAsync();
    public Task<Result<Receita>> GetReceitaByIdAsync(int id);
    public Task<Result<bool>> ExistReceitaAsync(string nome);
    public Task<Result> CreateReceitaAsync(Receita receita);
    public Task<Result> UpdateReceitaAsync(Receita receita);
    public Task<Result> DeleteReceitaAsync(Receita receita);
}
