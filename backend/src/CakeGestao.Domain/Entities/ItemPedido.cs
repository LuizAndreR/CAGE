namespace CakeGestao.Domain.Entities;

public class ItemPedido
{
    public int Id { get; set; }
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }

    public int PedidoId { get; set; }
    public int ReceitaId { get; set; } 

    public required virtual Pedido Pedido { get; set; }
    public required virtual Receita Receita { get; set; }
}
