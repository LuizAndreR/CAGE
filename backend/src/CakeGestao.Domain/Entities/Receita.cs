namespace CakeGestao.Domain.Entities;

public class Receita
{
    public int Id { get; set; }
    public string Nome { get; private set; }
    public string ModoPreparo { get; private set; }
    public decimal PrecoVenda { get; private set; }


    public ICollection<Ingrediente> Ingredientes { get; set; } = new List<Ingrediente>();
    public ICollection<ItemPedido> ItemPedidos { get; set; } = new List<ItemPedido>();
    
    public int EmpresaId { get; private set; }
    public virtual Empresa Empresa { get; set; } = null!;

    public Receita(string nome, string modoPreparo, decimal precoVenda, int empresaId)
    {
        Nome = nome;
        ModoPreparo = modoPreparo;
        PrecoVenda = precoVenda;
        EmpresaId = empresaId;
    }

    public void AtualizarReceita(string nome, string modoPreparo, decimal precoVenda)
    {
        Nome = nome;
        ModoPreparo = modoPreparo;
        PrecoVenda = precoVenda;
    }
}
