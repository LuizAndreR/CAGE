namespace CakeGestao.Application.Dtos.Requests.Receita;

public class CreateReceitaRequest
{
    public required string Nome { get; set; }
    public required string ModePreparo { get; set; }
    public decimal Preco { get; set; }
}