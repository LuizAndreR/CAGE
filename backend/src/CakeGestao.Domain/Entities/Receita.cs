namespace CakeGestao.Domain.Entities;

public class Receita
{
    public int Id { get; set; }
    public string Nome { get; private set; }
    public string ModoPreparo { get; private set; }
    public decimal PrecoVenda { get; private set; }
    public decimal PercentualCustoExtra { get; private set; }
    public decimal PercentualMargemLucro { get; private set; }
    public decimal CustoTotal { get; private set; }

    public bool Status { get; private set; }
    
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
        Status = true;
        CustoTotal = 0;
    }

    public void AtualizarReceita(string nome, string modoPreparo, decimal precoVenda)
    {
        Nome = nome;
        ModoPreparo = modoPreparo;
        PrecoVenda = precoVenda;
    }

    public void AlteraStatus(bool status)
    {
        this.Status = status;
    }
    
    public void AdicionarIngrediente(Ingrediente ingrediente)
    {
        _ingredientes.Add(ingrediente);
    }

    public void CalcularPrecificacao(decimal custoIngredientes, decimal percCustoExtra, decimal percMargemLucro, decimal precoVendaInformado)
    {
        PercentualCustoExtra = percCustoExtra;
        decimal valorCustoExtra = custoIngredientes * (PercentualCustoExtra / 100);
        CustoTotal = custoIngredientes + valorCustoExtra;

        PrecoVenda = precoVendaInformado;
        if (CustoTotal > 0)
        {
            PercentualMargemLucro = ((PrecoVenda / CustoTotal) - 1) * 100;
        }
        else
        {
            PercentualMargemLucro = percMargemLucro;
        }
    }

    public void LimparIngredientes()
    {
        _ingredientes.Clear();
    }

    public void AtualizarCustoTotal(decimal novoCusto)
    {
        CustoTotal = novoCusto;
    }
}
