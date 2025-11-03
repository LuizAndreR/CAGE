namespace CakeGestao.Domain.Entities;

public class TokenRefresh
{
    public int Id { get; set; }
    public required string RefreshToken { get; set; }
    public DateTime CreatedAt { get; set; } 
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }  
    public bool IsUsed { get; set; }  

    public int UsuarioId { get; set; } 
    public virtual Usuario Usuario { get; set; } = null!;  
}
