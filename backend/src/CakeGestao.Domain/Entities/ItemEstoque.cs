namespace CakeGestao.Domain.Entities;

public class ItemEstoque
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public decimal QuantidadeAtual { get; set; } = 0;
    public required string UnidadeMedida { get; set; }

    public ICollection<Ingrediente> Ingredientes { get; set; } = new List<Ingrediente>();   
}
