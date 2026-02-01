namespace CakeGestao.Application.Features.Financeiro.Common;

public class TransacaoResponse
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public string IsCancelado { get; set; } = string.Empty;
    public string MotivoCancelamento { get; set; } = string.Empty;
}