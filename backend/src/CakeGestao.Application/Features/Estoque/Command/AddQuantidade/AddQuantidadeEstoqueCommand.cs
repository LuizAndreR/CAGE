using FluentResults;
using MediatR;
using System.Text.Json.Serialization;

namespace CakeGestao.Application.Features.Estoque.Command.AddQuantidade;

public class AddQuantidadeEstoqueCommand : IRequest<Result>
{
    [JsonIgnore]
    public int EmpresaId { get; set; }

    [JsonIgnore]
    public int ItemId { get; set; }

    public decimal QuantidadeAdicionar { get; set; }
    public decimal Valor { get; set; }
}
