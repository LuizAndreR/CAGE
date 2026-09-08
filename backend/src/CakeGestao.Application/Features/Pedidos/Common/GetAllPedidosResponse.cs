namespace CakeGestao.Application.Features.Pedidos.Common;

public class GetAllPedidosResponse
{
    public int Id { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
    public DateTime? DataEntrega { get; set; }
    public decimal ValorTotal { get; set; }
    public string StatusPedido { get; set; } = string.Empty;
    public bool Pago { get; set; }
}