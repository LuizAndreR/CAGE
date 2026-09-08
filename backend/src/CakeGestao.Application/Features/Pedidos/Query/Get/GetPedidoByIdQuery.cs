using System.Text.Json.Serialization;
using CakeGestao.Application.Features.Pedidos.Common;
using FluentResults;
using MediatR;

namespace CakeGestao.Application.Features.Pedidos.Query.Get;

public class GetPedidoByIdQuery : IRequest<Result<GetPedidoResponse>>
{
    [JsonIgnore]
    public int EmpresaId { get; set; }
    public int Id { get; set; }
}