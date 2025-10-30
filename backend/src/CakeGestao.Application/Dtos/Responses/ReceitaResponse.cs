namespace CakeGestao.Application.Dtos.Responses;

public class ReceitaResponse
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required string ModoPreparo { get; set; }
    public decimal PrecoVenda { get; set; }
}
