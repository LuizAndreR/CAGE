using CakeGestao.Domain.Enun;

namespace CakeGestao.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
    public string SenhaHash { get; set; }
    public UserRole Role { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? UltimoLogin { get; set; }
    
    public int? EmpresaId { get; set; }
    public virtual Empresa? Empresa { get; set; }

    public Usuario(string nome, string email, string senhaHash, UserRole role, DateTime dataCriacao, int? empresaId)
    {
        Nome = nome;
        Email = email;
        SenhaHash = senhaHash;
        Role = role;
        DataCriacao = dataCriacao;
        EmpresaId = empresaId;
    }

    public void AtualizarUltimoLogin(DateTime ultimoLogin)
    {
        UltimoLogin = ultimoLogin;
    }
}
