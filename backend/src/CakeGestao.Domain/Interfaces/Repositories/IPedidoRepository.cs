using CakeGestao.Domain.Entities;
using FluentResults;

namespace CakeGestao.Domain.Interfaces.Repositories;

public interface IPedidoRepository
{
    public Task<Result> CreatePedidoAsync(Pedido pedido);
}