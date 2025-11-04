namespace CakeGestao.Application.Dtos.Requests.Usuario;

public class UpdateSenhaUsuarioRequest
{
    public required string SenhaAtual { get; set; }
    public required string NovaSenha { get; set; }
}
