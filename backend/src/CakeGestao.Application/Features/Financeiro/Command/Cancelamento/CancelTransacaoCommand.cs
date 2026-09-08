using FluentResults;
using MediatR;
using System.Text.Json.Serialization;

namespace CakeGestao.Application.Features.Financeiro.Command.Cancelamento;

public class CancelTransacaoCommand : IRequest<Result>
{
    [JsonIgnore]
    public int TransacaoId { get; set; }
    [JsonIgnore]
    public int EmpresaId { get; set; }
    [JsonIgnore]
    public int UsuarioId { get; set; }

    public string Motivo { get; set; } = string.Empty;
}
