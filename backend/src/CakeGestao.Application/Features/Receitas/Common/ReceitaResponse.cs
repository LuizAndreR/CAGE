namespace CakeGestao.Application.Features.Receitas.Common;

public class ReceitaResponse
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required string ModoPreparo { get; set; }
    public decimal PrecoVenda { get; set; }
    public decimal PercentualMargemLucro { get; private set; }
    public decimal CustoExtra { get; set; }
    public decimal CustoTotal { get; private set; }
    public bool Status { get; set; }

    public List<IngredienteResponse> Ingredientes { get; set; } = new();

    public int TotalIngredientes => Ingredientes?.Count ?? 0;
}

public class IngredienteResponse
{
    public List<AlternativaIngredienteResponse> Alternativas { get; set; } = new();
    public List<int> AlternativasIds { get; set; } = new();
    public string Marca { get; set; } = string.Empty;
    public int ItemId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public required string UnidadeMedida { get; set; }
}
public class AlternativaIngredienteResponse
{
    public int ItemId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string UnidadeMedida { get; set; } = string.Empty;
}
