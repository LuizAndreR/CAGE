using CakeGestao.Domain.Enum;

namespace CakeGestao.Domain.Entities;

public class TransacaoFinanceira
{
    public int Id { get; set; }
    public TipoTransacaoEnum Tipo { get; private set; }
    public CategoriasEnum Categoria { get; private set; }
    public decimal Valor { get; private set; }
    public DateTime Data { get; private set; }
    public string Descricao { get; private set; } = string.Empty;

    public bool IsCancelado { get; private set; }
    public DateTime? DataCancelamento { get; private set; }
    public string? MotivoCancelamento { get; private set; }
    public int? CanceladoPorUsuarioId { get; private set; }

    public int? PedidoId { get; private set; }
    public virtual Pedido Pedido { get; set; } = null!;
    
    public int EmpresaId { get; private set; }
    public virtual Empresa Empresa { get; set; } = null!;
    
    public TransacaoFinanceira(TipoTransacaoEnum tipo, CategoriasEnum categoria, decimal valor, DateTime data, string descricao, int? pedidoId, int empresaId)
    {
        Tipo = tipo;
        Categoria = categoria;
        Valor = valor;
        Data = data;
        Descricao = descricao;
        PedidoId = pedidoId;
        EmpresaId = empresaId;
        IsCancelado = false;
    }

    public void Cancelar(string motivo, int usuarioId)
    {
        IsCancelado = true;
        DataCancelamento = DateTime.UtcNow;
        MotivoCancelamento = motivo;
        CanceladoPorUsuarioId = usuarioId;
    }
}
