namespace CakeGestao.Domain.Entities;

public class Pedido
{
    public Guid Id { get; set; }
    public required string ClienteNome { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataPedido { get; set; }
    public DateTime DataEntrega { get; set; }
    public decimal ValorTotal { get; set; }
    public string Status { get; set; } = "Pendente";

    public ICollection<ItemPedido> Itens { get; set; } = new List<ItemPedido>();

    public virtual ICollection<TransacaoFinanceira> Transacoes { get; set; } = new List<TransacaoFinanceira>();
}
