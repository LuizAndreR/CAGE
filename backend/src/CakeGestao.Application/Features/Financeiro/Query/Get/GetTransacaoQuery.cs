using CakeGestao.Application.Features.Financeiro.Common;
using FluentResults;
using MediatR;

namespace CakeGestao.Application.Features.Financeiro.Query.Get;

public class GetTransacaoQuery : IRequest<Result<TransacaoResponse>>
{
    public int EmpresaId { get; set; }
    public int Id { get; set; }
}