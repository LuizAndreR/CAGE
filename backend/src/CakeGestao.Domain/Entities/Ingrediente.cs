using CakeGestao.Domain.Enum;

namespace CakeGestao.Domain.Entities;

public class Ingrediente
{
    public int Id { get; private set; } 
    public decimal Quantidade { get; private set; }
    public UnidadeMedidaEnum UnidadeMedida { get; private set; }

    public int ReceitaId { get; private set; }
    public virtual Receita Receita { get; private set; } = null!;

    public int ItemId { get; private set; }
    public virtual Estoque Item { get; private set; } = null!;

    
    public Ingrediente(int itemId, decimal quantidade, UnidadeMedidaEnum unidadeMedida)
    {
        ItemId = itemId;
        Quantidade = quantidade;
        UnidadeMedida = unidadeMedida;
    }

    public void AtualizarQuantidade(decimal novaQuantidade)
    {
        Quantidade = novaQuantidade;
    }
}