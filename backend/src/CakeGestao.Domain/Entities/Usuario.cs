using CakeGestao.Domain.Enun;

namespace CakeGestao.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required string Email { get; set; }
    public required string SenhaHash { get; set; }
    public UserRole Role { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? UltimoLogin { get; set; }
    
    public int EmpresaId { get; set; }
    public virtual required Empresa Empresa { get; set; }
}
