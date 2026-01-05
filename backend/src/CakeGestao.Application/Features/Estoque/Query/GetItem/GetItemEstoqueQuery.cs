using CakeGestao.Application.UseCases.Estoque.Common;
using FluentResults;
using MediatR;
using System.Text.Json.Serialization;

namespace CakeGestao.Application.Features.Estoque.Query.GetItem;

public class GetItemEstoqueQuery : IRequest<Result<ItemEstoqueResponse>>
{
    [JsonIgnore]
    public int EmpresaId { get; set; }

    public int ItemId { get; set; }
}
