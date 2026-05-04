using CakeGestao.Domain.Entities;
using FluentResults;

namespace CakeGestao.Domain.Interfaces.Repositories;

public interface IReceitaRepository
{
    public Task<Result<List<Receita>>> GetAllReceitasAsync(int empresaId);
    public Task<Result<Receita>> GetReceitaByIdAsync(int id, int empresaId);
    public Task<Result> CreateReceitaAsync(Receita receita);
    public Task<Result> UpdateReceitaAsync(Receita receita);
    public Task<Result> DeleteReceitaAsync(Receita receita);
}
