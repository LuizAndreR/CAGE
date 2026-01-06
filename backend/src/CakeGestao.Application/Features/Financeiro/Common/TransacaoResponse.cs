namespace CakeGestao.Application.Features.Financeiro.Common;

public class TransacaoResponse
{
    public string Tipo { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public DateTime Data { get; set; }
}