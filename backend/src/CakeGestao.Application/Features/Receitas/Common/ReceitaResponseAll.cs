namespace CakeGestao.Application.Features.Receitas.Common;

public class ReceitaResponseAll
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public decimal PrecoVenda { get; set; }
    public decimal CustoTotal { get; private set; }
    public bool Status { get; set; }
}
