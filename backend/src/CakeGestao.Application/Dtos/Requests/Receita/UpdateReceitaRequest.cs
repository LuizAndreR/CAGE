namespace CakeGestao.Application.Dtos.Requests.Receita;

public class UpdateReceitaRequest
{
    public required string Nome { get; set; }
    public required string ModoPreparo { get; set; }
    public decimal PrecoVenda { get; set; }
} 
