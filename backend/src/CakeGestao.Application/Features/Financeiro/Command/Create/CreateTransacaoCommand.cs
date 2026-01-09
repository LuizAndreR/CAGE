using System.Text.Json.Serialization;
using FluentResults;
using MediatR;

namespace CakeGestao.Application.Features.Financeiro.Command.Create;

public class CreateTransacaoCommand : IRequest<Result>
{
    [JsonIgnore]
    public int EmpresaId { get; set; }
    [JsonIgnore]
    public int? PedidoId { get; set; }
    
    public string Tipo { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime Data { get; set; }
    public string Descricao { get; set; } = string.Empty;
}
