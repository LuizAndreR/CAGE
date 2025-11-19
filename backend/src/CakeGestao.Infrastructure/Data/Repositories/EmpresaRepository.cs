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

    public EmpresaRepository(CageContext context, ILogger<EmpresaRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<List<Empresa>>> GetAllEmpresasAsync()
    {
        _logger.LogInformation("Buscando todas as empresas no banco de dados");
        var empresas = await _context.Empresas.ToListAsync();
        if (empresas.Count is 0)
        {
            _logger.LogInformation("Não foi entrcontado nenhuma empresa cadastrada no banco de dados.");
            return Result.Fail("Não foi entrcontado nenhuma empresa cadastrada no banco de dados.");
        }
        _logger.LogInformation("Total de {Count} empresas encontradas no banco de dados", empresas.Count);
        return Result.Ok(empresas);
    }
    
    public async Task<Result<Empresa>> GetEmpresaByIdAsync(int empresaId)
    {
        _logger.LogInformation("Buscando empresa com ID {EmpresaId} no banco de dados", empresaId);
        var empresa = await _context.Empresas.FindAsync(empresaId);
        if (empresa == null)
        {
            _logger.LogWarning("Empresa com ID {EmpresaId} não encontrada no banco de dados", empresaId);
            return Result.Fail<Empresa>($"Empresa com ID {empresaId} não encontrada.");
        }

        _logger.LogInformation("Empresa com ID {EmpresaId} encontrada no banco de dados", empresaId);
        return Result.Ok(empresa);
    }

    public async Task<Result> EmpresaExistsByNomeAsync(string nome)
    {
        _logger.LogInformation("Verificando existência de empresa com nome {Nome} no banco de dados", nome);

        var exists = await _context.Empresas.AnyAsync(e => e.Nome == nome);
        if (exists is true)
        {
            _logger.LogInformation("Empresa com nome {Nome} já existe no banco de dados", nome);
            return Result.Fail($"Empresa com nome {nome} já existe.");
        }

        _logger.LogInformation("Empresa com nome {Nome} não existe no banco de dados", nome);  
        return Result.Ok();
    }

    public async Task<Result> EmpresaExistsByIdAsync(int empresaId)
    {
        _logger.LogInformation("erificando existência de empresa com id {Id} no banco de dados", empresaId);

        var exists = await _context.Empresas.AnyAsync(e => e.Id == empresaId);
        if (exists is false)
        {
            _logger.LogInformation("Empresa com id {Id} não existe no banco de dados", empresaId); 
            return Result.Fail($"Empresa com id {empresaId} não existe no banco de dados");
        }
        _logger.LogInformation("Empresa com id {Id} já existe no banco de dados", empresaId);
        return Result.Ok();
    }

    public async Task CreateEmpresaAsync(Empresa empresa)
    {
        _logger.LogInformation("Adicionando nova empresa {Nome} ao banco de dados", empresa.Nome);
        _context.Empresas.Add(empresa);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Empresa {Nome} adicionada com sucesso ao banco de dados", empresa.Nome);
    }

    public async Task UpdateEmpresaAsync(Empresa empresa)
    {
        _logger.LogInformation("Iniciando atualização da empresa com ID {EmpresaId}", empresa.Id);

        _context.Empresas.Update(empresa);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Empresa com ID {EmpresaId} atualizada com sucesso", empresa.Id);
    }

    public async Task DeleteEmpresaAsync(Empresa empresa)
    {
        _logger.LogInformation("Iniciando delete da empresa com ID {EmpresaId}", empresa.Id);

        _context.Empresas.Remove(empresa);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Empresa com ID {EmpresaId} deletado com sucesso", empresa.Id);
    }
}
