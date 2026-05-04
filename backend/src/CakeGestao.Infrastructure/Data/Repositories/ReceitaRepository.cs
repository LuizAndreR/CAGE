using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Infrastructure.Data.Repositories;

public class ReceitaRepository : IReceitaRepository
{
    private readonly CageContext _context;
    private readonly ILogger<ReceitaRepository> _logger; 
    private const string LogPrefix = "[Receita Repository]";

    public ReceitaRepository(CageContext context, ILogger<ReceitaRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<List<Receita>>> GetAllReceitasAsync(int empresaId)
    {
        _logger.LogInformation("{LogPrefix} buscando todas as receitas do banco de dados da empresa: {EmpresaId} ", LogPrefix, empresaId);
        
        var receitas = await _context.Receitas.AsNoTracking().Where(r => r.EmpresaId == empresaId).ToListAsync();

        _logger.LogInformation("{LogPrefix} Busca concluída. Foram encontradas {Count} receitas.", LogPrefix, receitas.Count);
        return Result.Ok(receitas);
    }
    
    public async Task<Result<Receita>> GetReceitaByIdAsync(int id, int empresaId)
    {
        var receita = await _context.Receitas.Include(r => r.Ingredientes).FirstOrDefaultAsync(r => r.Id == id && r.EmpresaId == empresaId);
        if (receita == null)
        {
            _logger.LogWarning("{LogPrefix} Receita ID: {Id} da Empresa: {EmpresaId} não encontrada.", LogPrefix, id, empresaId); 
            return Result.Fail<Receita>("Receita não encontrada.");
        }
        return Result.Ok(receita);
    }
    
    public async Task<Result> CreateReceitaAsync(Receita receita)
    {
        _logger.LogInformation("{LogPrefix} Criando nova receita: {Nome}", LogPrefix, receita.Nome);

        _context.Receitas.Add(receita);
        await _context.SaveChangesAsync();

        return Result.Ok();
    }

    public async Task<Result> UpdateReceitaAsync(Receita receita)
    {
        _logger.LogInformation("{LogPrefix} Atualizando receita de ID: {Id}", LogPrefix, receita.Id);
        _context.Receitas.Update(receita);
        await _context.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> DeleteReceitaAsync(Receita receita)
    {
        _logger.LogInformation("{LogPrefix} Deletando receita de ID: {Id}", LogPrefix, receita.Id);
        _context.Receitas.Remove(receita);
        await _context.SaveChangesAsync();
        return Result.Ok();
    }
}
