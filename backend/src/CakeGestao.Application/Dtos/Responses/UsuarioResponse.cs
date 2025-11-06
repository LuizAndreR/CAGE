namespace CakeGestao.Application.Dtos.Responses;

public class UsuarioResponse
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required string Email { get; set; }
    public required string Role { get; set; }
    public DateTime DataInicio { get; set; }
}
