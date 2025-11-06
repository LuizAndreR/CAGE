namespace CakeGestao.Application.Dtos.Requests.Usuario;

public class UpdateFuncionarioUsuarioRequest
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required string Role { get; set; }
}
