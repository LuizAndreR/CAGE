namespace CakeGestao.Domain.Entities;

public class ItemPedido
{
    public int Id { get; set; }
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }

    public decimal Subtotal => Quantidade * ValorUnitario;

    public int PedidoId { get; set; }
    public int ReceitaId { get; set; } 

    public required virtual Pedido Pedido { get; set; }
    public required virtual Receita Receita { get; set; }

    public ItemPedido(int receitaId, int quantidade, decimal valorUnitario)
    {
        ReceitaId = receitaId;
        Quantidade = quantidade;
        ValorUnitario = valorUnitario;
    }
}
