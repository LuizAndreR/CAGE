using CakeGestao.Domain.Entities;
using FluentResults;

namespace CakeGestao.Domain.Interfaces.Repositories;

public interface IReceitaRepository
{
    public Task<Result<bool>> ExistReceitaAsync(string nome);
    public Task<Result> CreateReceitaAsync(Receita receita);
}
