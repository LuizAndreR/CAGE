namespace CakeGestao.Application.Features.Pedidos.Common;

public class GetPedidoResponse
{
    public int Id { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public string? TelefoneCliente { get; set; }
    public string? Descricao { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataEntrega { get; set; }
    public DateTime? DataPagamento { get; set; }
    public decimal ValorTotal { get; set; }
    public string StatusPedido { get; set; } = string.Empty;
    public bool Pago { get; set; }
    
    public List<ItemPedidoDetailDto> Itens { get; set; } = new();
}

public class ItemPedidoDetailDto
{
    public int ReceitaId { get; set; }
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal SubTotal { get; set; } 
}