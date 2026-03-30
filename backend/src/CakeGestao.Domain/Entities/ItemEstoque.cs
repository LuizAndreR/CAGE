using CakeGestao.Domain.Enum;

namespace CakeGestao.Domain.Entities;

public class ItemEstoque
{
    public int Id { get;  set; }
    public string Nome { get; private set; } = null!;
    public decimal QuantidadeAtual { get; private set; }
    public UnidadeMedidaEnum UnidadeMedida { get; private set; }
    public decimal QuantidadeMinina { get; private set; }
    public decimal ValorMedia { get; private set; }

    public UnidadeMedidaEnum? UnidadeReferenciaVolume { get; private set; }
    public decimal? PesoReferenciaEmGramas { get; private set; }
    

    public virtual ICollection<Ingrediente> Ingredientes { get; set; } = new List<Ingrediente>();

    public int EmpresaId { get; set; }
    public virtual Empresa Empresa{ get; set; } = null!;

    protected ItemEstoque() { }

    public ItemEstoque(string nome, decimal quantidadeAtual, UnidadeMedidaEnum unidadeMedida, decimal quantidadeMinina, decimal valorMedia, int empresaId)
    {
        Nome = nome;
        QuantidadeAtual = quantidadeAtual;
        UnidadeMedida = unidadeMedida;
        QuantidadeMinina = quantidadeMinina;
        ValorMedia = valorMedia;
        EmpresaId = empresaId;
    }

    public void AtualizarDadosCadastrais(string nome, decimal quantidadeAtual, decimal quantidadeMinima, UnidadeMedidaEnum unidade)
    {
        Nome = nome;
        QuantidadeAtual = quantidadeAtual;
        QuantidadeMinina = quantidadeMinima;
        UnidadeMedida = unidade;
    }

    public void AdicionarQuantidade (decimal quantidadeEntrada, decimal valorUnitarioEntrada)
    {
        decimal valorTotalAtualNoEstoque = QuantidadeAtual * ValorMedia;
        decimal valorTotalDaEntrada = quantidadeEntrada * valorUnitarioEntrada;

        decimal novaQuantidadeTotal = QuantidadeAtual + quantidadeEntrada;

        if (novaQuantidadeTotal > 0)
        {
            ValorMedia = (valorTotalAtualNoEstoque + valorTotalDaEntrada) / novaQuantidadeTotal;
        }

        QuantidadeAtual = novaQuantidadeTotal;
    }

    public void RemoverQuantidade(decimal quantidadeRemover)
    {
        QuantidadeAtual -= quantidadeRemover;
    }
    
    public void AtualizarReferenciaDensidade(UnidadeMedidaEnum unidadeRef, decimal pesoEmGramas)
    {
        UnidadeReferenciaVolume = unidadeRef;
        PesoReferenciaEmGramas = pesoEmGramas;
    }
}
