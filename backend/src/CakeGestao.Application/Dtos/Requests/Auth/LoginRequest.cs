namespace CakeGestao.Application.Dtos.Requests.Auth;

public class LoginRequest
{
    public required string Email { get; set; }
    public required string Senha { get; set; }
}
