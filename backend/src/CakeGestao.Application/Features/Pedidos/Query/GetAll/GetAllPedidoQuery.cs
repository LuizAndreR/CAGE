using System.Text.Json.Serialization;
using CakeGestao.Application.Features.Pedidos.Common;
using FluentResults;
using MediatR;

namespace CakeGestao.Application.Features.Pedidos.Query.GetAll;

public class GetAllPedidoQuery : IRequest<Result<List<GetAllPedidosResponse>>>
{
    [JsonIgnore] 
    public int EmpresaId { get; set; }
}