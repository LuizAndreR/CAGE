using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Interfaces.Repositories;
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
}
