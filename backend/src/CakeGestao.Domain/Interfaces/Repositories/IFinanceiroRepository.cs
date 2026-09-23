using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Enum;
using FluentResults;

namespace CakeGestao.Domain.Interfaces.Repositories;

public interface IFinanceiroRepository
{
    public Task<decimal> GetTotalPorTipoMesAtualAsync(TipoTransacaoEnum tipo, int empresaId,
        CancellationToken cancellationToken);
    public Task<Result<List<TransacaoFinanceira>>> GetAllTransacoesAsync(int empresaId, TipoTransacaoEnum? tipo,
        CategoriasEnum? categoria, int? mes, int? ano);
    public Task<Result<decimal>> GetEntradaAsync(int empresaId);
    public Task<Result<decimal>> GetSaidaAsync(int empresaId);
    public Task<Result<TransacaoFinanceira>> GetTransacaoByPedidoId(int pedidoId, int empresaId);
    public Task<Result<TransacaoFinanceira>> GetTransacaoAsync(int id, int? empresaId);
    public Task CreateTransacaoAsync(TransacaoFinanceira transacao);
    public Task UpdateTransacaoAsync(TransacaoFinanceira transacao);
}
