using CakeGestao.Application.Features.Financeiro.Common;
using FluentResults;
using MediatR;

namespace CakeGestao.Application.Features.Financeiro.Query.GetEntrada;

public class GetFinanceiroResumoQuery : IRequest<Result<TransacaoResumo>>
{
    public int EmpresaId { get; set; }
}
