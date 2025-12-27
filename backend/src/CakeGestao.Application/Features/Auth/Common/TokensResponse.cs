namespace CakeGestao.Application.Features.Auth.Common;

public class TokensResponse
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}
