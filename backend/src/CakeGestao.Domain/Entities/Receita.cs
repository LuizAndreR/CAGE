namespace CakeGestao.Domain.Entities;

public class Receita
{
    public int Id { get; set; }
    public string Nome { get; private set; }
    public string ModoPreparo { get; private set; }
    public decimal PrecoVenda { get; private set; }

    public decimal CustoTotalEstimado { get; private set; }
    public bool Ativo { get; private set; }
    
    public int EmpresaId { get; private set; }
    public virtual Empresa Empresa { get; private set; } = null!;
    
    private readonly List<Ingrediente> _ingredientes = new();
    public IReadOnlyCollection<Ingrediente> Ingredientes => _ingredientes.AsReadOnly();
    
    public ICollection<ItemPedido> ItemPedidos { get; set; } = new List<ItemPedido>();
    
    public Receita(string nome, string modoPreparo, decimal precoVenda, int empresaId)
    {
        Nome = nome;
        ModoPreparo = modoPreparo;
        PrecoVenda = precoVenda;
        EmpresaId = empresaId;
        Ativo = true; 
        CustoTotalEstimado = 0;
    }

    public void AtualizarReceita(string nome, string modoPreparo, decimal precoVenda)
    {
        Nome = nome;
        ModoPreparo = modoPreparo;
        PrecoVenda = precoVenda;
    }

    public void Inativar()
    {
        Ativo = false;
    }

    public void Ativar()
    {
        Ativo = true;
    }
    
    public void AdicionarIngrediente(Ingrediente ingrediente)
    {
        _ingredientes.Add(ingrediente);
    }

    public void RemoverIngrediente(int ingredienteId)
    {
        var ingrediente = _ingredientes.FirstOrDefault(i => i.Id == ingredienteId);
        if (ingrediente != null)
        {
            _ingredientes.Remove(ingrediente);
        }
    }

    public void AtualizarCustoTotal(decimal novoCusto)
    {
        CustoTotalEstimado = novoCusto;
    }

    public void LimparIngredientes()
    {
        _ingredientes.Clear();
    }
}
