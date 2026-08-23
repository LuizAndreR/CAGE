namespace CakeGestao.Application.Features.Receitas.Common;

public class ReceitaResponse
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required string ModoPreparo { get; set; }
    public decimal PrecoVenda { get; set; }
    public decimal PercentualMargemLucro { get; private set; }
    public decimal PercentualCustoExtra { get; private set; }
    public decimal CustoTotal { get; private set; }
    public bool Status { get; set; }

    public List<IngredienteResponse> Ingredientes { get; set; } = new();

    public int TotalIngredientes => Ingredientes?.Count ?? 0;
}

public class IngredienteResponse
{
    public int ItemId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public required string UnidadeMedida { get; set; }
}