using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Infrastructure.Data.Repositories;

public class EstoqueRepository : IEstoqueRepository
{
    private readonly CageContext _context;
    private readonly ILogger<EstoqueRepository> _logger;

    public EstoqueRepository(CageContext context, ILogger<EstoqueRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<ItemEstoque>> GetItemEstoqueByIdAsync(int id)
    {
        _logger.LogInformation("Buscando ItemEstoque com ID: {Id}", id);
        var itemEstoque = await _context.ItensEstoque.FindAsync(id);
        if (itemEstoque == null)
        {
            _logger.LogWarning("ItemEstoque com ID: {Id} não encontrado.", id);
            return Result.Fail<ItemEstoque>("ItemEstoque não encontrado.");
        }
        _logger.LogInformation("ItemEstoque com ID: {Id} encontrado com sucesso.", id);
        return Result.Ok(itemEstoque);
    }

    public async Task<Result> ExistItemByNome(string nome)
    {
        _logger.LogInformation("Verificando existência de ItemEstoque com nome: {Nome}", nome);
        var exists = await _context.ItensEstoque.AnyAsync(e => e.Nome == nome);

        if (exists)
        {
            _logger.LogInformation("ItemEstoque com nome: {Nome} já existe.", nome);
            return Result.Ok();
        }

        _logger.LogInformation("ItemEstoque com nome: {Nome} não encontrado.", nome);
        return Result.Fail("ItemEstoque não encontrado.");
    }   

    public async Task CreateItemEstoqueAsync(ItemEstoque itemEstoque)
    {
        _logger.LogInformation("Criando nova ItemEstoque: {Nome}", itemEstoque.Nome);
        _context.ItensEstoque.Add(itemEstoque);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateItemEstoqueAsync(ItemEstoque itemEstoque)
    {
        _logger.LogInformation("Atualizando ItemEstoque com ID: {Id}", itemEstoque.Id);
        _context.ItensEstoque.Update(itemEstoque);
        await _context.SaveChangesAsync();
    }
}
