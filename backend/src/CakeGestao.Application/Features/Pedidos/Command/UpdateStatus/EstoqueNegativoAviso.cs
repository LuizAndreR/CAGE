using ItemEstoque = CakeGestao.Domain.Entities.Estoque;
using CakeGestao.Domain.Entities;
using FluentResults;

namespace CakeGestao.Application.Features.Pedidos.Command.UpdateStatus;

public class EstoqueNegativoAviso : Success
{
    public int ItemId { get; }
    public string Nome { get; }
    public string Marca { get; }
    public decimal Saldo { get; }
    public string UnidadeMedida { get; }

    public EstoqueNegativoAviso(ItemEstoque item) : base($"{item.Nome} — {item.Marca} ficou com estoque negativo.")
    {
        ItemId = item.Id;
        Nome = item.Nome;
        Marca = item.Marca;
        Saldo = item.QuantidadeAtual;
        UnidadeMedida = item.UnidadeMedida.ToString();
    }
}
