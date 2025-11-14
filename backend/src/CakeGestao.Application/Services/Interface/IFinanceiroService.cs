using CakeGestao.Application.Dtos.Requests.Transacao;
using FluentResults;

namespace CakeGestao.Application.Services.Interface;

public interface IFinanceiroService
{
    public Task<Result> CreateTransacaoAsync(CreateTransacaoRequest request, int empresaId, int? pedidoId);
}
