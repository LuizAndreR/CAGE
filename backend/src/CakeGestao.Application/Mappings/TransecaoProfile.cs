using AutoMapper;
using CakeGestao.Application.Dtos.Requests.Transacao;
using CakeGestao.Domain.Entities;

namespace CakeGestao.Application.Mappings;

public class TransecaoProfile : Profile
{
    public TransecaoProfile()
    {
        CreateMap<TransacaoFinanceira, CreateTransacaoRequest>()
            .ReverseMap()
            .ForMember(dest => dest.EmpresaId, opt => opt.Ignore())
            .ForMember(dest => dest.PedidoId, opt => opt.Ignore());
    }
}
