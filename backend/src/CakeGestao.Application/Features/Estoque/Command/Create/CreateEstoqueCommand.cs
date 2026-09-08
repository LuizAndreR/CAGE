using System.Text.Json.Serialization;
using FluentResults;
using MediatR;

namespace CakeGestao.Application.Features.Estoque.Command.Create;

public class CreateEstoqueCommand : IRequest<Result>
{
    [JsonIgnore]
    public int EmpresaId { get; set; }

    public required string Nome { get; set; }
    public required string Marca { get; set; }

    public decimal QuantidadeAtual { get; set; }
    public decimal Valor { get; set; }

    public required string UnidadeMedida { get; set; }

    public decimal QuantidadeMinima { get; set; }

    public decimal? PesoReferenciaEmGramas { get; set; }

    public string? UnidadeMedidaReferenciaVolume { get; set; }
}
