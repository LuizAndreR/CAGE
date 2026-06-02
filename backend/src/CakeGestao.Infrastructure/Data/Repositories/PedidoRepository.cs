using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Infrastructure.Data.Repositories;

public class PedidoRepository : IPedidoRepository
{
    private readonly CageContext _context;
    private readonly ILogger<EmpresaRepository> _logger;
    private const string LogPrefix = "[Pedido Repository]";

    public PedidoRepository(CageContext context, ILogger<EmpresaRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<List<Pedido>>> GetAllByEmpresaIdAsync(int empresaId)
    {
        var listPedido =  await _context.Pedidos
            .AsNoTracking() 
            .Where(p => p.EmpresaId == empresaId)
            .OrderByDescending(p => p.DataCriacao) 
            .ToListAsync();

        if (listPedido.Count <= 0)
        {
            return Result.Fail<List<Pedido>>("Nenhum pedido encontrado");
        }
        
        return Result.Ok(listPedido);
    }
    public async Task<Result<Pedido>> GetByIdAsync(int id, int empresaId)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.Id == id && p.EmpresaId == empresaId);

        if (pedido == null)
        {
            return Result.Fail($"Pedido de Id {id} não encontrado");
        }
        
        return Result.Ok(pedido);
    }
    
    public async Task<Result> CreatePedidoAsync(Pedido pedido)
    {
        _logger.LogInformation("{LogPrefix} Criando pedido de id: {Id}", LogPrefix, pedido.Id);

        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();

        return Result.Ok();
    }

    public async Task<Result> UpdatePedidoAsync(Pedido pedido)
    {
        _logger.LogInformation("{LogPrefix} Atualizando o pedido de id: {Id}", LogPrefix, pedido.Id);
        
        _context.Pedidos.Update(pedido);
        await _context.SaveChangesAsync();
        
        return Result.Ok(); 
    }
}