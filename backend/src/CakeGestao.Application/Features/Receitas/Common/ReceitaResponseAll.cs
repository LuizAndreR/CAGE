namespace CakeGestao.Application.Features.Receitas.Common;

public class ReceitaResponseAll
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public decimal PrecoVenda { get; set; }
}
