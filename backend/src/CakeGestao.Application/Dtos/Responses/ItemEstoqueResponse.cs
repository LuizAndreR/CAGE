namespace CakeGestao.Application.Dtos.Responses;

public class ItemEstoqueResponse
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required decimal QuantidadeAtual { get; set; }
    public required string UnidadeMedida { get; set; }
    public required decimal ValorMedia { get; set; }
}
