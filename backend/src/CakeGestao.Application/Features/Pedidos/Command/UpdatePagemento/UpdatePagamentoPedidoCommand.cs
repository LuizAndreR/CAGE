using System.Text.Json.Serialization;
using FluentResults;
using MediatR;

namespace CakeGestao.Application.Features.Pedidos.Command.UpdatePagemento;

public class UpdatePagamentoPedidoCommand : IRequest<Result>
{
    [JsonIgnore]
    public int EmpresaId { get; set; }

    [JsonIgnore]
    public int PedidoId { get; set; }
    
    [JsonIgnore]
    public int UsuarioId { get; set; }

    public bool Pagamento { get; set; }
}