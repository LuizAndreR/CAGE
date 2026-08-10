using CakeGestao.Application.Features.Dashboard.Common;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Dashboard.Query;

public class GetDashboardResumoHandler : IRequestHandler<GetDashboardResumoQuery, Result<DashboardResponseDto>>
{
    private readonly IEstoqueRepository _estoqueRepository;
    private readonly IFinanceiroRepository _financeiroRepository;
    private readonly IReceitaRepository _receitaRepository;
    private readonly IPedidoRepository _pedidoRepository;
    private readonly ILogger<GetDashboardResumoHandler> _logger;
    private const string LogPrefix = "[Get Dashboard Handler]";

    public GetDashboardResumoHandler(IEstoqueRepository estoqueRepository, IFinanceiroRepository financeiroRepository, IReceitaRepository receitaRepository, IPedidoRepository pedidoRepository, ILogger<GetDashboardResumoHandler> logger)
    {
        _estoqueRepository = estoqueRepository;
        _financeiroRepository = financeiroRepository;
        _receitaRepository = receitaRepository;
        _pedidoRepository = pedidoRepository;
        _logger = logger;
    }

    public async Task<Result<DashboardResponseDto>> Handle(GetDashboardResumoQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Iniciando montagem do dashboard para a EmpresaId: {EmpresaId}", LogPrefix, request.EmpresaId);
        
        var totalEstoque = await _estoqueRepository.GetTotalItensEstoqueAsync(request.EmpresaId, cancellationToken);
        var totalEntradas = await _financeiroRepository.GetTotalPorTipoMesAtualAsync(TipoTransacaoEnum.Entrada, request.EmpresaId, cancellationToken);
        var totalSaidas = await _financeiroRepository.GetTotalPorTipoMesAtualAsync(TipoTransacaoEnum.Saida, request.EmpresaId, cancellationToken);
        var ultimasReceitas = await _receitaRepository.GetUltimasReceitasAsync(request.EmpresaId, 5, cancellationToken);
        var ultimosPedidos = await _pedidoRepository.GetUltimosPedidosAsync(request.EmpresaId, 5, cancellationToken);
        
        var response = ConstruirDtoDeResposta(
            request.NomeUsuario,
            totalEstoque, 
            totalEntradas, 
            totalSaidas, 
            ultimasReceitas, 
            ultimosPedidos
        );
        
        _logger.LogInformation("{LogPrefix} Dashboard montado com sucesso para a EmpresaId: {EmpresaId}. Total Estoque: {Estoque} | Saldo: {Saldo}", 
            LogPrefix, request.EmpresaId, response.TotalItensEstoque, response.Financeiro.SaldoAtual);

        return Result.Ok(response);
    }

    private DashboardResponseDto ConstruirDtoDeResposta(string nomeUsuario, int totalEstoque, decimal totalEntradas, decimal totalSaidas, IEnumerable<Domain.Entities.Receita> ultimasReceitas, IEnumerable<Domain.Entities.Pedido> ultimosPedidos)
    {
        return new DashboardResponseDto
        {
            NomeUsuario = nomeUsuario,
            TotalItensEstoque = totalEstoque,
            Financeiro = new FinanceiroResumoDto
            {
                TotalEntradas = totalEntradas,
                TotalSaidas = totalSaidas,
                SaldoAtual = totalEntradas - totalSaidas
            },
            UltimasReceitas = ultimasReceitas.Select(r => new UltimaReceitaDto
            {
                ReceitaId = r.Id,
                Nome = r.Nome,
                PrecoVenda = r.PrecoVenda
            }).ToList(),
            UltimosPedidos = ultimosPedidos.Select(p => new UltimoPedidoDto
            {
                PedidoId = p.Id,
                ClienteNome = p.ClienteNome,
                ValorTotal = p.ValorTotal,
                DataCriacao = p.DataCriacao 
            }).ToList()
        };
    }
}