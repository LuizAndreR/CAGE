using CakeGestao.Domain.Enum;

namespace CakeGestao.Application.UseCases.Estoque.Common;

public class ItemEstoqueResponse
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required string Marca { get; set; }
    public required decimal QuantidadeAtual { get; set; }
    public required string UnidadeMedida { get; set; }
    public required decimal ValorMedia { get; set; }
    public decimal QuantidadeMinina { get; set; }

    public string? UnidadeReferenciaVolume { get; set; }
    public decimal? PesoReferenciaEmGramas { get; set; }
}
