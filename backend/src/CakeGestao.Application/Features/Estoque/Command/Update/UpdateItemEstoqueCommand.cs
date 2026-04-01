using FluentResults;
using MediatR;
using System.Text.Json.Serialization;

namespace CakeGestao.Application.Features.Estoque.Command.Update;

public class UpdateItemEstoqueCommand : IRequest<Result>
{
    [JsonIgnore]
    public int EmpresaId { get; set; }

    [JsonIgnore]
    public int ItemId { get; set; }

    public required string Nome { get; set; }
    public decimal QuantidadeAtual { get; set; }
    public decimal QuantidadeMinima { get; set; }
    public required string UnidadeMedida { get; set; }
    
    public decimal? PesoReferenciaEmGramas { get; set; }

    public string? UnidadeMedidaReferenciaVolume { get; set; }
}
