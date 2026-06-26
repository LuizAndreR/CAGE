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
    private const string LogPrefix = "[Estoque Repository]";

    public EstoqueRepository(CageContext context, ILogger<EstoqueRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Estoque>> GetItemEstoqueByIdAsync(int itemId, int? empresaId)
    {
        _logger.LogDebug("{LogPrefix} Buscando ItemEstoque com ID: {Id} (Filtro Empresa: {Empresa}).", LogPrefix, itemId, empresaId);
        var query = _context.Estoque.AsQueryable();
        if (empresaId.HasValue)
        {
            query = query.Where(x => x.EmpresaId == empresaId.Value);
        }

        var itemEstoque = await query.FirstOrDefaultAsync(x => x.Id == itemId);

        if (itemEstoque == null)
        {
            _logger.LogWarning("{LogPrefix} Item ID {Id} não encontrado (Filtro Empresa: {Empresa}).", LogPrefix, itemId, empresaId);
            return Result.Fail<Estoque>("ItemEstoque não encontrado.");
        }

        return Result.Ok(itemEstoque);
    }

    public async Task<Result<List<Estoque>>> GetItensByIdsAsync(IEnumerable<int> ids, int empresaId)
    {
        _logger.LogInformation("{LogPrefix} Buscando uma lista de itens cadastrados no banco de dados para a EmpresaId: {EmpresaId}", LogPrefix, empresaId);
        
        var listaItens = await _context.Estoque
            .Where(i => i.EmpresaId == empresaId && ids.Contains(i.Id))
            .ToListAsync();
        
        return Result.Ok(listaItens);
    }

    public async Task<Result<List<Estoque>>> GetAllItemEstoqueByEmpresaIdAsync(int empresaId)
    {
        _logger.LogDebug("{LogPrefix} Listando estoque. EmpresaId: {Id}", LogPrefix, empresaId);
        var listItemEstoque = await _context.Estoque.AsNoTracking().Where(i => i.EmpresaId == empresaId).ToListAsync();
        if (listItemEstoque.Count == 0)
        {
            _logger.LogInformation("{LogPrefix} Nenhum item cadastrado para esta empresa.", LogPrefix);
            return Result.Fail<List<Estoque>>("ItemEstoque não encontrado.");
        }

        return Result.Ok(listItemEstoque);
    }

    public async Task<Result<List<Estoque>>> GetAlertaEstoqueByEmpresaIdAsync(int empresaId, int quantidadeMinima)
    {
        _logger.LogDebug("{LogPrefix} Verificando alertas. Limite Global: {Qtd}", LogPrefix, quantidadeMinima);

        var alertaEstoque = await _context.Estoque.AsNoTracking().Where(i => i.EmpresaId == empresaId && i.QuantidadeAtual <= quantidadeMinima).ToListAsync();
        _logger.LogInformation("Lista de alerta {list}", alertaEstoque);
        if (alertaEstoque.Count == 0)
        {
            _logger.LogInformation("{LogPrefix} Estoque saudável. Nenhum item abaixo do limite.", LogPrefix);
            return Result.Fail<List<Estoque>>("Nenhum item com alerta de estoque encontrado.");
        }
        else
        {
            _logger.LogWarning("{LogPrefix} ALERTA: {Count} itens com estoque baixo.", LogPrefix, alertaEstoque.Count);
        }

        return Result.Ok(alertaEstoque);
    }

    public async Task<Result> ExistItemByNome(string nome, int empresaId, string marca)
    {
        _logger.LogDebug("{LogPrefix} Verificando existência de ItemEstoque com nome: {Nome} e marca: {Marca}", LogPrefix, nome, marca);

        var nomeNormalizado = nome.Trim().ToLower();
        var marcaNormalizada = marca.Trim().ToLower();
        var exists = await _context.Estoque.AnyAsync(e => e.Nome.ToLower() == nomeNormalizado && e.Marca.ToLower() == marcaNormalizada && e.EmpresaId == empresaId);

        if (exists)
        {
            _logger.LogWarning("{LogPrefix} Conflito: Item '{Nome}' já existe.", LogPrefix, nome);
            return Result.Ok();
        }

        return Result.Fail("ItemEstoque não encontrado.");
    }   
    
    public async Task CreateItemEstoqueAsync(Estoque itemEstoque)
    {
        _logger.LogInformation("{LogPrefix} Criando item: {Nome}", LogPrefix, itemEstoque.Nome);
        _context.Estoque.Add(itemEstoque);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateItemEstoqueAsync(Estoque itemEstoque)
    {
        _logger.LogInformation("{LogPrefix} Atualizando item ID: {Id}", LogPrefix, itemEstoque.Id);
        _context.Estoque.Update(itemEstoque);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteItemEstoqueAsync(Estoque itemEstoque)
    {
        _logger.LogInformation("{LogPrefix} Excluindo item ID: {Id}", LogPrefix, itemEstoque.Id);
        _context.Estoque.Remove(itemEstoque);
        await _context.SaveChangesAsync();
    }
}
