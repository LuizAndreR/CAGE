namespace CakeGestao.Application.Dtos.Responses;

public class EmpresaResponse
{
    public int Id { get; set; }
    public required string Nome { get; set; }   
    public required string Emdereco { get; set; }
    public DateTime DataCadastro { get; set; }
    public required string Status { get; set; }
}