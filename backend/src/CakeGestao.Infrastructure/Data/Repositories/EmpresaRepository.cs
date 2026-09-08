using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Infrastructure.Data.Repositories;

public class EmpresaRepository : IEmpresaRepository
{
    private readonly CageContext _context;
    private readonly ILogger<EmpresaRepository> _logger;
    private const string LogPrefix = "[Empresa Repository]";

    public EmpresaRepository(CageContext context, ILogger<EmpresaRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<List<Empresa>>> GetAllEmpresasAsync()
    {
        _logger.LogDebug("{LogPrefix} Listando todas as empresas.", LogPrefix);
        var empresas = await _context.Empresas.AsNoTracking().ToListAsync();
        if (empresas.Count is 0)
        {
            _logger.LogInformation("{LogPrefix} Nenhuma empresa cadastrada no momento.", LogPrefix); ;
            return Result.Fail("Não foi entrcontado nenhuma empresa cadastrada no banco de dados.");
        }
        _logger.LogInformation("{LogPrefix} {Count} empresas encontradas no banco de dados", LogPrefix, empresas.Count);
        return Result.Ok(empresas);
    }
    
    public async Task<Result<Empresa>> GetEmpresaByIdAsync(int empresaId)
    {
        _logger.LogInformation("{LogPrefix} Buscando empresa com ID {EmpresaId}", LogPrefix, empresaId);
        var empresa = await _context.Empresas.FindAsync(empresaId);
        if (empresa == null)
        {
            _logger.LogWarning("{LogPrefix} Empresa ID {Id} não encontrada.", LogPrefix, empresaId);
            return Result.Fail<Empresa>($"Empresa com ID {empresaId} não encontrada.");
        }

        return Result.Ok(empresa);
    }

    public async Task<Result> EmpresaExistsByNomeAsync(string nome)
    {
        _logger.LogInformation("{LogPrefix} Verificando existência de empresa com nome {Nome}", LogPrefix, nome);

        var exists = await _context.Empresas.AsNoTracking().AnyAsync(e => e.Nome == nome);
        if (exists)
        {
            _logger.LogWarning("{LogPrefix} Conflito: Já existe empresa com nome '{Nome}'.", LogPrefix, nome);
            return Result.Fail($"Empresa com nome {nome} já existe.");
        }
  
        return Result.Ok();
    }

    public async Task CreateEmpresaAsync(Empresa empresa)
    {
        _logger.LogInformation("{LogPrefix} Criando nova empresa: {Nome}", LogPrefix, empresa.Nome);
        _context.Empresas.Add(empresa);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateEmpresaAsync(Empresa empresa)
    {
        _logger.LogInformation("{LogPrefix} Atualizando empresa ID: {Id}", LogPrefix, empresa.Id);

        _context.Empresas.Update(empresa);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteEmpresaAsync(Empresa empresa)
    {
        _logger.LogInformation("{LogPrefix} Excluindo empresa ID: {Id}", LogPrefix, empresa.Id);

        _context.Empresas.Remove(empresa);
        await _context.SaveChangesAsync();
    }
}
