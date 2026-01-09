using CakeGestao.Domain.Entities;
using FluentResults;

namespace CakeGestao.Domain.Interfaces.Repositories;

public interface IFinanceiroRepository
{
    public Task CreateTransacaoAsync(TransacaoFinanceira transacao);
    public Task<Result<List<TransacaoFinanceira>>> GetAllTransacoesAsync(int empresaId);
    public Task<Result<TransacaoFinanceira>> GetTransacaoAsync(int id);
}
