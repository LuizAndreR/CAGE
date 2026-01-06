using CakeGestao.Application.Features.Financeiro.Common;
using FluentResults;
using MediatR;

namespace CakeGestao.Application.Features.Financeiro.GetAll;

public class GetAllTransacaoQuery : IRequest<Result<List<TransacaoResponse>>>
{
    public int EmpresaId { get; set; }
}