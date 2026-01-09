using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Infrastructure.Data.Repositories;

public class FinanceiroRepository : IFinanceiroRepository
{
    private readonly CageContext _context;
    private readonly ILogger<FinanceiroRepository> _logger;

    public FinanceiroRepository(CageContext context, ILogger<FinanceiroRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task CreateTransacaoAsync(TransacaoFinanceira transacao)
    {
        _logger.LogInformation("Criando nova transação finaceira no banco de dados de {Tipo} no valor {Valor}", transacao.Tipo, transacao.Valor);

        _context.TransacoesFinanceiras.Add(transacao);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Transação financeira criada com sucesso com ID {Id}", transacao.Id);
    }

    public async Task<Result<List<TransacaoFinanceira>>> GetAllTransacoesAsync(int empresaId)
    {
        _logger.LogInformation("Iniciando a busca de todas transações da empresa de id: {Id}", empresaId);
        var listTransacoes = await _context.TransacoesFinanceiras.AsNoTracking().Where(i => i.EmpresaId == empresaId).ToListAsync();
        if (listTransacoes == null)
        {
            _logger.LogWarning("Nenhuma transação encontrada no banco de dados da empresa de id: {Id}", empresaId);
            return Result.Fail("Nenhuma transação encontrada no banco de dados");
        }
        _logger.LogInformation("Foi encontrada no total de {Transações} no banco de dados da empresa de id: {Id}", listTransacoes.Count, empresaId);
        return Result.Ok(listTransacoes);
    }

    public async Task<Result<TransacaoFinanceira>> GetTransacaoAsync(int id)
    {
        _logger.LogInformation("Iniciando busca da transação de id: {Id} no banco de dados", id);

        var transacao = await _context.TransacoesFinanceiras.FindAsync(id);
        if (transacao == null)
        {
            _logger.LogWarning("Transação de id: {Id} não encontrado no banco de  dados", id);
            return Result.Fail("Transação não encontrado no banco de dados");
        }
        
        _logger.LogInformation("Transação de id: {Id} encontrado no banco de dados", id);
        return Result.Ok(transacao);
    }
}
