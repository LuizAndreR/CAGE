using CakeGestao.Application.Features.Financeiro.Common;
using FluentResults;
using MediatR;
using System.Text.Json.Serialization;

namespace CakeGestao.Application.Features.Financeiro.Query.GetAll;

public class GetAllTransacaoQuery : IRequest<Result<List<TransacaoResponse>>>
{
    [JsonIgnore]
    public int EmpresaId { get; set; }

    public string? Tipo { get; set; } 
    public string? Categoria { get; set; }
    public int? Mes { get; set; }
    public int? Ano  { get; set; } 
}