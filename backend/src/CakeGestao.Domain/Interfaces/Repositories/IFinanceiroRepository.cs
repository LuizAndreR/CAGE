using CakeGestao.Domain.Entities;

namespace CakeGestao.Domain.Interfaces.Repositories;

public interface IFinanceiroRepository
{
    public Task CreateTransacaoAsync(TransacaoFinanceira transacao);
}
