using CakeGestao.Domain.Enum;

namespace CakeGestao.Domain.Entities;

public class ItemEstoque
{
    public int Id { get;  set; }
    public string Nome { get; private set; } = null!;
    public decimal QuantidadeAtual { get; private set; }
    public UnidadeMedidaEnum UnidadeMedida { get; private set; }
    public decimal ValorMedia { get; private set; }    

    public virtual ICollection<Ingrediente> Ingredientes { get; set; } = new List<Ingrediente>();

    public int EmpresaId { get; set; }
    public virtual Empresa Empresa{ get; set; } = null!;

    protected ItemEstoque() { }

    public ItemEstoque(string nome, decimal quantidadeAtual, UnidadeMedidaEnum unidadeMedida, decimal valorMedia, int empresaId)
    {
        Nome = nome;
        QuantidadeAtual = quantidadeAtual;
        UnidadeMedida = unidadeMedida;
        ValorMedia = valorMedia;
        EmpresaId = empresaId;
    }

    public void AdicionarQuantidade (decimal quantidadeEntrada, decimal valorUnitarioEntrada)
    {
        if (quantidadeEntrada <= 0)
            throw new ArgumentException("A quantidade a adicionar deve ser maior que zero.", nameof(quantidadeEntrada));

        if (valorUnitarioEntrada < 0)
            throw new ArgumentException("O valor unitário não pode ser negativo.", nameof(valorUnitarioEntrada));

        decimal valorTotalAtualNoEstoque = this.QuantidadeAtual * this.ValorMedia;
        decimal valorTotalDaEntrada = quantidadeEntrada * valorUnitarioEntrada;

        decimal novaQuantidadeTotal = this.QuantidadeAtual + quantidadeEntrada;

        if (novaQuantidadeTotal > 0)
        {
            this.ValorMedia = (valorTotalAtualNoEstoque + valorTotalDaEntrada) / novaQuantidadeTotal;
        }

        this.QuantidadeAtual = novaQuantidadeTotal;
    }

    public void RemoverQuantidade(decimal quantidadeRemover)
    {
        this.QuantidadeAtual -= quantidadeRemover;
    }
}
