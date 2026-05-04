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
    
    public Receita(string nome, string modoPreparo, int empresaId)
    {
        Nome = nome;
        ModoPreparo = modoPreparo;
        EmpresaId = empresaId;
        Status = true;
    }

    public void AtualizarReceita(string nome, string modoPreparo)
    {
        Nome = nome;
        ModoPreparo = modoPreparo;
    }

    public void AlteraStatus(bool status)
    {
        Status = status;
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

        if (precoVendaInformado > 0)
        {
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
        else
        {
            PercentualMargemLucro = percMargemLucro;

            if (CustoTotal > 0)
            {
                PrecoVenda = CustoTotal + (CustoTotal * (PercentualMargemLucro / 100));
            }
            else
            {
                PrecoVenda = 0;
            }
        }
        
    }

    public void LimparIngredientes()
    {
        _ingredientes.Clear();
    }
}
