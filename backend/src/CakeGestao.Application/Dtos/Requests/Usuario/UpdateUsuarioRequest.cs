namespace CakeGestao.Application.Dtos.Requests.Usuario;

public class UpdateUsuarioRequest
{
    public required string Nome { get; set; }
    public required string Email { get; set; }
}