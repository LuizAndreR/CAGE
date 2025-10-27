namespace CakeGestao.Domain.Entities;

public class ItemPedido
{
    public Guid Id { get; set; }
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }

    public Guid PedidoId { get; set; }
    public Guid ReceitaId { get; set; } 

    public required virtual Pedido Pedido { get; set; }
    public required virtual Receita Receita { get; set; }
}
