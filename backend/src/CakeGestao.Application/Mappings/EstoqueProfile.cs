using AutoMapper;
using CakeGestao.Application.Dtos.Requests.Estoque;
using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Enum;

namespace CakeGestao.Application.Mappings;

public class EstoqueProfile : Profile
{
    public EstoqueProfile()
    {
        CreateMap<CreateEstoqueRequest, ItemEstoque>()
            .ForMember(dest => dest.UnidadeMedida, opt => opt.MapFrom(src =>
                Enum.Parse<UnidadeMedidaEnum>(src.UnidadeMedida, true)))
            .ForMember(dest => dest.ValorMedia, opt => opt.MapFrom(src => src.Valor));
    }
}
