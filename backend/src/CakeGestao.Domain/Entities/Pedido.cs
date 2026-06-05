using CakeGestao.Domain.Enum;

namespace CakeGestao.Domain.Entities;

public class Pedido
{
    public int Id { get; set; }
    public string ClienteNome { get; private set; }
    public string? TelefoneCliente { get; private set; }
    public string? Descricao { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataEntrega { get; private set; }
    public decimal ValorTotal { get; private set; }
    public StatusPedidoEnum StatusPedido { get; private set; }

    public bool Pago { get; private set; }
    public DateTime? DataPagamento { get; private set; }


    public int EmpresaId { get; set; }
    public virtual Empresa Empresa { get; set; } = null!;

    public ICollection<ItemPedido> Itens { get; set; } = new List<ItemPedido>();

    public virtual ICollection<TransacaoFinanceira> Transacoes { get; set; } = new List<TransacaoFinanceira>();

    public Pedido(string clienteNome, string? telefoneCliente, string? descricao, DateTime? dataEntrega, int empresaId)
    {
        ClienteNome = clienteNome;
        TelefoneCliente = telefoneCliente;
        Descricao = descricao;
        DataEntrega = dataEntrega;
        EmpresaId = empresaId;

        DataCriacao = DateTime.UtcNow;
        StatusPedido = StatusPedidoEnum.Pendente;
        Pago = false; 
        ValorTotal = 0;
    }
    
    public void Atualizar(string clienteNome, string? telefoneCliente, string? descricao, DateTime? dataEntrega)
    {
        ClienteNome = clienteNome;
        TelefoneCliente = telefoneCliente;
        Descricao = descricao;
        DataEntrega = dataEntrega;
    }
    
    public void LimparItens()
    {
        Itens.Clear();
        ValorTotal = 0;
        Pago = false;
        DataPagamento = null;
    }
    
    public void AdicionarItem(ItemPedido item)
    {
        Itens.Add(item);
        CalcularValorTotal();
    }

    private void CalcularValorTotal()
    {
        ValorTotal = Itens.Sum(i => i.Subtotal);
    }
    
    public void AlterarStatus(StatusPedidoEnum novoStatus)
    {
        StatusPedido = novoStatus;
    }

    public void InformarPagamento()
    {
        Pago = true;
        DataPagamento = DateTime.UtcNow;
    }

    public void EstornarPagamento()
    {
        Pago = false;
        DataPagamento = null;
    }
}
