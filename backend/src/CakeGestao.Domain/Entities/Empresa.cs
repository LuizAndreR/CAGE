namespace CakeGestao.Domain.Entities;

public class Empresa
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public DateTime DataCadastro { get; set; }
    public required string Status { get; set; }

    public required ICollection<Receita> Receitas { get; set; }
    public required ICollection<Pedido> Pedidos { get; set; }
    public required ICollection<TransacaoFinanceira> TransacaoFinanceiras { get; set; }
    public required ICollection<Usuario> Usuarios { get; set; }
    public required ICollection<ItemEstoque> ItemEstoques { get; set; }
}