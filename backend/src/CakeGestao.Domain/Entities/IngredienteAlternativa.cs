namespace CakeGestao.Domain.Entities;

public class IngredienteAlternativa
{
    public int Id { get; private set; }
    public int IngredienteId { get; private set; }
    public int ItemId { get; private set; }
    public int Ordem { get; private set; }
    public Estoque Item { get; private set; } = null!;

    public IngredienteAlternativa(int itemId, int ordem)
    {
        ItemId = itemId;
        Ordem = ordem;
    }
}
