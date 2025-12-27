using FluentResults;
using MediatR;
using System.Text.Json.Serialization;

namespace CakeGestao.Application.Features.Estoque.Command.Delete;

public class DeleteItemEstoqueCommand : IRequest<Result>
{
    [JsonIgnore]
    public int EmpresaId { get; set; }

    public int ItemId { get; set; }
}
