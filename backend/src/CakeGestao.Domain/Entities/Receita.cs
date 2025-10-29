namespace CakeGestao.Domain.Entities;

public class Receita
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required string ModoPreparo { get; set; }
    public decimal PrecoVenda { get; set; } = 0;


    public ICollection<Ingrediente> Ingredientes { get; set; } = new List<Ingrediente>();
    public ICollection<ItemPedido> ItemPedidos { get; set; } = new List<ItemPedido>();
}
