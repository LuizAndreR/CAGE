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

    public ReceitaRepository(CageContext context, ILogger<ReceitaRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<List<Receita>>> GetAllReceitasAsync()
    {
        _logger.LogInformation("buscando todas as receitas do banco de dados");
        
        var receitas = await _context.Receitas.ToListAsync();

        if (receitas.Count == 0)
        {
            _logger.LogInformation("Nenhuma receita foi encontrada.");
            return Result.Fail<List<Receita>>("Nenhuma receita foi encontrada.");
        }
        
        return Result.Ok(receitas);
    }
    
    public async Task<Result<Receita>> GetReceitaByIdAsync(int id)
    {
        _logger.LogInformation("Buscando receita por ID: {Id}", id);
        var receita = await _context.Receitas.FindAsync(id);
        if (receita == null)
        {
            _logger.LogWarning("Receita com ID: {Id} não encontrada", id);
            return Result.Fail<Receita>($"Receita com ID {id} não encontrada.");
        }
        return Result.Ok(receita);
    }

    public async Task<Result<bool>> ExistReceitaAsync(string nome)
    {
        _logger.LogInformation("Verificando existência de receita de nome: {Nome}", nome);

        var exists = await _context.Receitas.AnyAsync(r => r.Nome == nome);

        return Result.Ok(exists);
    }

    public async Task<Result> CreateReceitaAsync(Receita receita)
    {
        _logger.LogInformation("Criando receita de nome: {Nome}", receita.Nome);

        _context.Receitas.Add(receita);
        await _context.SaveChangesAsync();

        return Result.Ok();
    }
}
