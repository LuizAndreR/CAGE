using CakeGestao.Application.UseCases.Estoque.Common;
using FluentResults;
using MediatR;
using System.Text.Json.Serialization;

namespace CakeGestao.Application.UseCases.Estoque.Alerta;

public class GetAlertaEstoqueQuery : IRequest<Result<List<ItemEstoqueResponse>>>
{
    [JsonIgnore]
    public int EmpresaId { get; set; }
}
