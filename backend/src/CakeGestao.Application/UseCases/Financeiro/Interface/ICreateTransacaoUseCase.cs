using CakeGestao.Application.Dtos.Requests.Transacao;
using FluentResults;

namespace CakeGestao.Application.UseCases.Financeiro.Interface;

public interface ICreateTransacaoUseCase
{
    public Task<Result> ExecuteAsync(CreateTransacaoRequest request, int empresaId, int? pedidoId);
}
