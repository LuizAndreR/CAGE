using CakeGestao.Domain.Enum;

namespace CakeGestao.Domain.Entities;

public class Pedido
{
    public int Id { get; set; }
    public required string ClienteNome { get; set; }
    public string? TelefoneCliente { get; private set; }
    public string? Descricao { get; set; }
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataEntrega { get; private set; }

    public decimal ValorTotal { get; set; }
    public StatusPedidoEnum StatusPedido { get; set; }

    public bool Pago { get; private set; }
    public DateTime? DataPagamento { get; private set; }


    public int EmpresaId { get; set; }
    public virtual required Empresa Empresa { get; set; }

    public ICollection<ItemPedido> Itens { get; set; } = new List<ItemPedido>();

    public virtual ICollection<TransacaoFinanceira> Transacoes { get; set; } = new List<TransacaoFinanceira>();

    public Pedido(string clienteNome, string? telefoneCliente, DateTime? dataEntrega, int empresaId)
    {
        ClienteNome = clienteNome;
        TelefoneCliente = telefoneCliente;
        DataEntrega = dataEntrega;
        EmpresaId = empresaId;

        DataCriacao = DateTime.UtcNow;
        StatusPedido = StatusPedidoEnum.Pendente;
        Pago = false; 
        ValorTotal = 0;
    }
}
