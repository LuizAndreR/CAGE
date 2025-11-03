namespace CakeGestao.Domain.Interfaces.Security;

public interface IJwtTokenService
{
    public Task<(string accessToken, string refreshToken)> TokenService(int usuarioId, string email, string role);
}
