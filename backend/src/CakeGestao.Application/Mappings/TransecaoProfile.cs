using AutoMapper;
using CakeGestao.Application.Dtos.Requests.Transacao;
using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Enum;

namespace CakeGestao.Application.Mappings;

public class TransecaoProfile : Profile
{
    public TransecaoProfile()
    {
        CreateMap<CreateTransacaoRequest, TransacaoFinanceira>()
            .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src =>
                Enum.Parse<TipoTransacaoEnum>(src.Tipo, true)))
            .ForMember(dest => dest.Categoria, opt => opt.MapFrom(src =>
                Enum.Parse<CategoriasEnum>(src.Categoria, true)))
            //.ForMember(dest => dest.PedidoId, opt => opt.MapFrom(src => src.PedidoId ?? 0));
            .ForMember(dest => dest.EmpresaId, opt => opt.Ignore())
            .ForMember(dest => dest.PedidoId, opt => opt.Ignore());
    }
}
