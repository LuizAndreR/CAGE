using CakeGestao.Domain.Enum;

namespace CakeGestao.Domain.Entities;

// Quantidade efetivamente retirada, na unidade do item no momento da produção.
public class ConsumoEstoquePedido
{
    public int Id { get; private set; }
    public int PedidoId { get; private set; }
    public int ItemId { get; private set; }
    public decimal Quantidade { get; private set; }
    public UnidadeMedidaEnum UnidadeMedida { get; private set; }

    public ConsumoEstoquePedido(int itemId, decimal quantidade, UnidadeMedidaEnum unidadeMedida)
    {
        ItemId = itemId;
        Quantidade = quantidade;
        UnidadeMedida = unidadeMedida;
    }
}
