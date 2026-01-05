using CakeGestao.Application.UseCases.Estoque.Common;
using FluentResults;
using MediatR;
using System.Text.Json.Serialization;

namespace CakeGestao.Application.Features.Estoque.Query.GetAll;
public class GetAllItemEstoqueQuery : IRequest<Result<List<ItemEstoqueResponse>>>
{
    [JsonIgnore]
    public int EmpresaId { get; set; }

}
