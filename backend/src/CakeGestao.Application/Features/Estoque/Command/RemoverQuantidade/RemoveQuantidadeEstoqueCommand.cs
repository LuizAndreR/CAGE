using FluentResults;
using MediatR;
using System.Text.Json.Serialization;

namespace CakeGestao.Application.Features.Estoque.Command.RemoverQuantidade;

public class RemoveQuantidadeEstoqueCommand : IRequest<Result>
{
    [JsonIgnore]
    public int EmpresaId { get; set; }
    [JsonIgnore]
    public int ItemId { get; set; }

    public required decimal QuantidadeARemover { get; set; }
}
