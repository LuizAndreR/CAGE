namespace CakeGestao.Application.Features.Estoque.Common;

public class ItemEstoqueResponseAlert
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required string Marca { get; set; }
    public required decimal QuantidadeAtual { get; set; }
    public required decimal QuantidadeMinina { get; set; }
    public required string UnidadeMedida { get; set; }
    public required decimal ValorMedia { get; set; }
}
            