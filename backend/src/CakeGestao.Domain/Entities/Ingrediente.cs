namespace CakeGestao.Domain.Entities;

public class Ingrediente
{
    public int Id { get; set; }
    public required decimal Quantidade { get; set; } = 0;
    public required string UnidadeMedida { get; set; }

    public int ReceitaId { get; set; }
    public required virtual Receita Receita { get; set; }

    public int ItemId { get; set; }
    public required virtual ItemEstoque Item { get; set; }  
}
