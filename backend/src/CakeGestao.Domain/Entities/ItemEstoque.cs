namespace CakeGestao.Domain.Entities;

public class ItemEstoque
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public decimal QuantidadeAtual { get; set; }
    public required string UnidadeMedida { get; set; }

    public ICollection<Ingrediente> Ingredientes { get; set; } = new List<Ingrediente>();

    public int EmpresaId { get; set; }
    public virtual required Empresa Empresa{ get; set; }
}
