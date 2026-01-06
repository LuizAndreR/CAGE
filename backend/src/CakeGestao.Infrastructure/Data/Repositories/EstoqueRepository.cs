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

    public async Task<Result<List<ItemEstoque>>> GetAllItemEstoqueByEmpresaIdAsync(int empresaId)
    {
        _logger.LogInformation("Buscando todo os itens do estoque da empresa de id: {Id}", empresaId);
        var listItemEstoque = await _context.ItensEstoque.AsNoTracking().Where(i => i.EmpresaId == empresaId).ToListAsync();
        if (listItemEstoque == null)
        {
            _logger.LogInformation("Nenhum item encontrado cadastrado no banco de dados da empresa de id: {Id}", empresaId);
            return Result.Fail<List<ItemEstoque>>("ItemEstoque não encontrado.");
        }
        _logger.LogInformation("Encontrado o total de {Total} itens cadastrados no banco de dados da empresa de id: {Id}", listItemEstoque.Count, empresaId);
        return Result.Ok(listItemEstoque);
    }

    public async Task<Result<List<ItemEstoque>>> GetAlertaEstoqueByEmpresaIdAsync(int empresaId, int quantidadeMinima)
    {
        _logger.LogInformation("Buscando itens do estoque com alerta para a empresa de id: {Id}", empresaId);
        var alertaEstoque = await _context.ItensEstoque.AsNoTracking().Where(i => i.EmpresaId == empresaId && i.QuantidadeAtual <= quantidadeMinima).ToListAsync();
        if (alertaEstoque == null)
        {
            _logger.LogInformation("Nenhum item com alerta de estoque encontrado para a empresa de id: {Id}", empresaId);
            return Result.Fail<List<ItemEstoque>>("Nenhum item com alerta de estoque encontrado.");
        }
        return Result.Ok(alertaEstoque);
    }


    public async Task<Result> ExistItemByNome(string nome, int empresaId)
    {
        _logger.LogInformation("Verificando existência de ItemEstoque com nome: {Nome}", nome);

        var nomeNormalizado = nome.Trim().ToLower();
        var exists = await _context.ItensEstoque.AnyAsync(e => e.Nome.ToLower() == nomeNormalizado && e.EmpresaId == empresaId);

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

    public async Task DeleteItemEstoqueAsync(ItemEstoque itemEstoque)
    {
        _logger.LogInformation("Deletando ItemEstoque com ID: {Id}", itemEstoque.Id);
        _context.ItensEstoque.Remove(itemEstoque);
        await _context.SaveChangesAsync();
    }
}
