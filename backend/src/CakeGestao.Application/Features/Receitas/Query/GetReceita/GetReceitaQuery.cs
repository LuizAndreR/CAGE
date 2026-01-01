using CakeGestao.Application.Dtos.Responses;
using FluentResults;
using MediatR;

namespace CakeGestao.Application.Features.Receitas.Query.GetReceita;

public class GetReceitaQuery : IRequest<Result<ReceitaResponse>>
{
    public int EmpresaId { get; set; }
    public int Id { get; set; }
}