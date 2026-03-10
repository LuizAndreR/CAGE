using CakeGestao.Domain.Enum;

namespace CakeGestao.Domain.Entities;

public class Empresa
{
    public int Id { get; set; }
    public string Nome { get; set; }   
    public string Endereco { get; set; }
    public DateTime DataCadastro { get; set; }
    public StatusEmpresaEnum Status { get; set; }

    public virtual ICollection<Receita> Receitas { get; set; } = new List<Receita>();
    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    public virtual ICollection<TransacaoFinanceira> TransacaoFinanceiras { get; set; } = new List<TransacaoFinanceira>();
    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    public virtual ICollection<ItemEstoque> ItemEstoques { get; set; } = new List<ItemEstoque>();

    public Empresa(string nome, string endereco, StatusEmpresaEnum status)
    {
        Nome = nome;
        Endereco = endereco;
        DataCadastro = DateTime.UtcNow;
        Status = status;
    }

    public void AtualizarDadosCadastrais(string nome, string endereco)
    {
        Nome = nome;
        Endereco = endereco;
    }

    public void AtulizarStatus(StatusEmpresaEnum status)
    {
        Status = status;
    }
}