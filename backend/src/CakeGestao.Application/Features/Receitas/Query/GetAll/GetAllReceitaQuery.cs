using CakeGestao.Application.Features.Receitas.Common;
using FluentResults;
using MediatR;
using System.Text.Json.Serialization;

namespace CakeGestao.Application.Features.Receitas.Query.GetAll;

public class GetAllReceitaQuery : IRequest<Result<List<ReceitaResponseAll>>>
{
    [JsonIgnore]
    public int EmpresaId { get; set; }
}