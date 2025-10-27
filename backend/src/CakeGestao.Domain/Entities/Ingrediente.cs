namespace CakeGestao.Domain.Entities;

public class Ingrediente
{
    public Guid Id { get; set; }
    public required decimal Quantidade { get; set; } = 0;
    public required string UnidadeMedida { get; set; }

    public Guid ReceitaId { get; set; }
    public required virtual Receita Receita { get; set; }

    public Guid ItemId { get; set; }
    public required virtual ItemEstoque Item { get; set; }  
}
