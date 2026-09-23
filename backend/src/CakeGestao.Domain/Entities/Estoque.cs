using CakeGestao.Domain.Enum;

namespace CakeGestao.Domain.Entities;

public class Estoque
{
    public int Id { get;  set; }
    public string Nome { get; private set; } = null!;
    public string Marca { get; set; }
    public decimal QuantidadeAtual { get; private set; }
    public UnidadeMedidaEnum UnidadeMedida { get; private set; }
    public decimal QuantidadeMinina { get; private set; }
    public decimal ValorMedia { get; private set; }

    public UnidadeMedidaEnum? UnidadeReferenciaVolume { get; private set; }
    public decimal? PesoReferenciaEmGramas { get; private set; }
    

    public virtual ICollection<Ingrediente> Ingredientes { get; set; } = new List<Ingrediente>();

    public int EmpresaId { get; set; }
    public virtual Empresa Empresa{ get; set; } = null!;

    public Estoque() { }

    public Estoque(string nome, string marca, decimal quantidadeAtual, UnidadeMedidaEnum unidadeMedida, decimal quantidadeMinina, decimal valorMedia, int empresaId, UnidadeMedidaEnum? unidadeMedidaEnum, decimal? pesoReferenciaEmGramas)
    {
        Nome = nome;
        Marca = marca;
        QuantidadeAtual = quantidadeAtual;
        UnidadeMedida = unidadeMedida;
        QuantidadeMinina = quantidadeMinina;
        ValorMedia = valorMedia;
        EmpresaId = empresaId;
        UnidadeReferenciaVolume = unidadeMedidaEnum;
        PesoReferenciaEmGramas = pesoReferenciaEmGramas;
    }

    public void AtualizarDadosCadastrais(string nome, string marca, decimal quantidadeAtual, decimal quantidadeMinima, UnidadeMedidaEnum unidade, UnidadeMedidaEnum? unidadeMedidaEnum, decimal? pesoReferenciaEmGramas)
    {
        Nome = nome;
        Marca = marca;
        QuantidadeAtual = quantidadeAtual;
        QuantidadeMinina = quantidadeMinima;
        UnidadeMedida = unidade;
        UnidadeReferenciaVolume = unidadeMedidaEnum;
        PesoReferenciaEmGramas = pesoReferenciaEmGramas;
    }

    public void AdicionarQuantidade(decimal quantidadeEntrada, decimal valorTotalEntrada)
    {
        decimal valorTotalAtualNoEstoque = QuantidadeAtual * ValorMedia;
        
        decimal novaQuantidadeTotal = QuantidadeAtual + quantidadeEntrada;

        if (novaQuantidadeTotal > 0)
        {
            ValorMedia = (valorTotalAtualNoEstoque + valorTotalEntrada) / novaQuantidadeTotal;
        }

        QuantidadeAtual = novaQuantidadeTotal;
    }   

    public void RemoverQuantidade(decimal quantidadeRemover)
    {
        QuantidadeAtual -= quantidadeRemover;
    }
}
