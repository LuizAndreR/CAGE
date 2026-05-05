using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
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

    public async Task<Result> CreatePedidoAsync(Pedido pedido)
    {
        _logger.LogInformation("{LogPrefix} Criando pedido de id: {Id}", LogPrefix, pedido.Id);
        
        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();
        
        return Result.Ok();
    }
}