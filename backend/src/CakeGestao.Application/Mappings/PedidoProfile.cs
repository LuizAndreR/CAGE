using AutoMapper;
using CakeGestao.Application.Features.Pedidos.Common;
using CakeGestao.Domain.Entities;

namespace CakeGestao.Application.Mappings;

public class PedidoProfile : Profile
{
    public PedidoProfile()
    {
        CreateMap<Pedido, GetAllPedidosResponse>();
    }
}