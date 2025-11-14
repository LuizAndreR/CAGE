using CakeGestao.Application.Dtos.Requests.Transacao;
using CakeGestao.Application.Services.Interface;
using CakeGestao.Application.UseCases.Financeiro.Interface;
using FluentResults;

namespace CakeGestao.Application.Services.Service;

public class FinanceiroService : IFinanceiroService
{
    private readonly ICreateTransacaoUseCase _createTransacaoUseCase;

    public FinanceiroService(ICreateTransacaoUseCase createTransacaoUseCase)
    {
        _createTransacaoUseCase = createTransacaoUseCase;
    }

    public async Task<Result> CreateTransacaoAsync(CreateTransacaoRequest request, int empresaId, int? pedidoId) =>  await _createTransacaoUseCase.ExecuteAsync(request, empresaId, pedidoId);
}
