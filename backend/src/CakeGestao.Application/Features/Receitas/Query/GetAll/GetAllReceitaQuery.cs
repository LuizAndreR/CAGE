using CakeGestao.Application.Features.Receitas.Common;
using FluentResults;
using MediatR;

namespace CakeGestao.Application.Features.Receitas.Query.GetAll;

public class GetAllReceitaQuery : IRequest<Result<List<ReceitaResponse>>>
{
    public int EmpresaId { get; set; }
}