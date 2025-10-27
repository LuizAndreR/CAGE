using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
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

    public async Task<Result> CreateReceitaAsync(Receita receita)
    {
        _logger.LogInformation("Criando receita de nome: {Nome}", receita.Nome);

        _context.Receitas.Add(receita);
        await _context.SaveChangesAsync();

        return Result.Ok();
    }
}
