namespace CakeGestao.Application.Dtos.Responses;

public class RefreshTokenResponce
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}
