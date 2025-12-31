using System.Text.Json.Serialization;
using CakeGestao.Application.UseCases.Estoque.Common;
using FluentResults;
using MediatR;

namespace CakeGestao.Application.Features.Estoque.Query.Alerta;

public class GetAlertaEstoqueQuery : IRequest<Result<List<ItemEstoqueResponse>>>
{
    [JsonIgnore]
    public int EmpresaId { get; set; }
}
