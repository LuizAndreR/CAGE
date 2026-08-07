namespace CakeGestao.Application.Features.Dashboard.Common;

public class DashboardResponseDto
{
    public int TotalItensEstoque { get; set; }
    public FinanceiroResumoDto Financeiro { get; set; } = new();
    public List<UltimaReceitaDto> UltimasReceitas { get; set; } = new();
    public List<UltimoPedidoDto> UltimosPedidos { get; set; } = new();
}

public class FinanceiroResumoDto
{
    public decimal TotalEntradas { get; set; }
    public decimal TotalSaidas { get; set; }
    public decimal SaldoAtual { get; set; }
}

public class UltimaReceitaDto
{
    public int ReceitaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal PrecoVenda { get; set; }
}

public class UltimoPedidoDto
{
    public int PedidoId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public DateTime DataCriacao { get; set; }
}