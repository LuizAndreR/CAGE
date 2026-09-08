using CakeGestao.Application.Features.Estoque.Common;
using CakeGestao.Application.UseCases.Estoque.Common;
using FluentResults;
using MediatR;
using System.Text.Json.Serialization;

namespace CakeGestao.Application.Features.Estoque.Query.Alerta;

public class GetAlertaEstoqueQuery : IRequest<Result<List<ItemEstoqueResponseAlert>>>
{
    [JsonIgnore]
    public int EmpresaId { get; set; }
}
