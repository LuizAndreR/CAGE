namespace CakeGestao.Application.Dtos.Responses;

public class TokensResponce
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}
