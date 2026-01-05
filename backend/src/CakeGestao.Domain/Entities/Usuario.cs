using CakeGestao.Domain.Enun;

namespace CakeGestao.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; private set; }
    public string Email { get; private set; }
    public string SenhaHash { get; private set; }
    public UserRole Role { get; private set; }
    public DateTime DataCriacao { get; private set; }
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

    public void AtualizarFuncionario(string nome, UserRole role)
    {
        Nome = nome;
        Role = role;
    }
    
    public void AtualizarUsuario(string nome, string email)
    {
        Nome = nome;
        Email = email;
    }

    public void AlterarSenhaHash(string senhaHash)
    {
        SenhaHash = senhaHash;
    }
    
    public void AtualizarUltimoLogin(DateTime ultimoLogin)
    {
        UltimoLogin = ultimoLogin;
    }
}
