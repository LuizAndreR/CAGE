using CakeGestao.Domain.Entities;
using FluentResults;

namespace CakeGestao.Domain.Interfaces.Repositories;

public interface IPedidoRepository
{
    public Task<Result> CreatePedidoAsync(Pedido pedido);
    public Task<Result<List<Pedido>>> GetAllByEmpresaIdAsync(int empresaId);
    public Task<Result<Pedido>> GetByIdAsync(int id, int empresaId);
}