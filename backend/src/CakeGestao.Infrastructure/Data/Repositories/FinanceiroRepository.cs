using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Infrastructure.Data.Repositories;

public class FinanceiroRepository : IFinanceiroRepository
{
    private readonly CageContext _context;
    private readonly ILogger<FinanceiroRepository> _logger;
    private const string LogPrefix = "[Financeiro Repository]";

    public FinanceiroRepository(CageContext context, ILogger<FinanceiroRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<List<TransacaoFinanceira>>> GetAllTransacoesAsync(int empresaId, TipoTransacaoEnum? tipo, CategoriasEnum? categoria, int? mes, int? ano)
    {
        _logger.LogDebug("{LogPrefix} Listando financeiro. EmpresaId: {Id}", LogPrefix, empresaId);

        var query = _context.TransacoesFinanceiras.AsNoTracking()
                .Where(i => i.EmpresaId == empresaId);

        if (tipo.HasValue)
        {
            query = query.Where(x => x.Tipo == tipo);
        }

        if (categoria.HasValue)
        {
            query = query.Where(x => x.Categoria == categoria);
        }
        
        var listTransacoes = await query.OrderByDescending(x => x.Data).ToListAsync();
        
        if (listTransacoes.Count == 0)
        {
            _logger.LogInformation("{LogPrefix} Nenhuma transação registrada.", LogPrefix);
            return Result.Fail("Nenhuma transação encontrada no banco de dados");
        }

        return Result.Ok(listTransacoes);
    }

    public async Task<Result<decimal>> GetEntradaAsync(int empresaId)
    {
        _logger.LogDebug("{LogPrefix} Calculando total de entradas. EmpresaId: {Id}", LogPrefix, empresaId);

        var totalEntrada = await _context.TransacoesFinanceiras
            .AsNoTracking()
            .Where(x => x.EmpresaId == empresaId && x.Tipo == TipoTransacaoEnum.Entrada && x.IsCancelado == false)
            .SumAsync(x => x.Valor);

        if (totalEntrada == 0)
        {
            _logger.LogInformation("{LogPrefix} Nenhuma entrada registrada.", LogPrefix);
            return Result.Fail("Nenhuma entrada encontrada no banco de dados");
        }

        return Result.Ok(totalEntrada);
    }

    public async Task<Result<decimal>> GetSaidaAsync(int empresaId)
    {
        _logger.LogDebug("{LogPrefix} Calculando total de saídas. EmpresaId: {Id}", LogPrefix, empresaId);

        var totalSaida = await _context.TransacoesFinanceiras
            .AsNoTracking()
            .Where(x => x.EmpresaId == empresaId && x.Tipo == TipoTransacaoEnum.Saida && x.IsCancelado == false)
            .SumAsync(x => x.Valor);

        if (totalSaida == 0)
        {
            _logger.LogInformation("{LogPrefix} Nenhuma saída registrada.", LogPrefix);
            return Result.Fail("Nenhuma saída encontrada no banco de dados");
        }

        return Result.Ok(totalSaida);
    }

    public async Task<Result<TransacaoFinanceira>> GetTransacaoAsync(int id, int? empresaId)
    {
        _logger.LogDebug("{LogPrefix} Buscando transação ID {Id}. EmpresaId: {EmpresaId}", LogPrefix, id, empresaId);

        var query = _context.TransacoesFinanceiras.AsQueryable();

        if (empresaId.HasValue)
        {
            query = query.Where(x => x.EmpresaId == empresaId.Value);
        }
        var transacao = await query.FirstOrDefaultAsync(x => x.Id == id);

        if (transacao == null)
        {
            _logger.LogWarning("{LogPrefix} Transação ID {Id} não encontrada.", LogPrefix, id);
            return Result.Fail("Transação não encontrado no banco de dados");
        }

        return Result.Ok(transacao);
    }

    public async Task CreateTransacaoAsync(TransacaoFinanceira transacao)
    {
        _logger.LogInformation("{LogPrefix} Registrando transação: {Tipo} | Valor: {Valor}", LogPrefix, transacao.Tipo, transacao.Valor);

        _context.TransacoesFinanceiras.Add(transacao);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateTransacaoAsync(TransacaoFinanceira transacao)
    {
        _logger.LogInformation("{LogPrefix} Atualizando transação ID {Id}", LogPrefix, transacao.Id);

        _context.TransacoesFinanceiras.Update(transacao);
        await _context.SaveChangesAsync();
    }
}
