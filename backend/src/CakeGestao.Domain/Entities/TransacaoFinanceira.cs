namespace CakeGestao.Domain.Entities;

public class TransacaoFinanceira
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty; // "Entrada" ou "Saída"
    public decimal Valor { get; set; } = 0;
    public DateTime Data { get; set; }
    public string Descricao { get; set; } = string.Empty;

    public int PedidoId { get; set; }
    public virtual Pedido Pedido { get; set; } = null!;
    
    public int EmpresaId { get; set; }
    public virtual required Empresa Empresa { get; set; }
}
