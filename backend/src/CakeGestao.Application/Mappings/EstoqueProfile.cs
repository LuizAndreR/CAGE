using AutoMapper;
using CakeGestao.Application.UseCases.Estoque.Common;
using CakeGestao.Application.UseCases.Estoque.Update;
using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Enum;

namespace CakeGestao.Application.Mappings;

public class EstoqueProfile : Profile
{
    public EstoqueProfile()
    {
        CreateMap<ItemEstoque, ItemEstoqueResponse>();
        CreateMap<UpdateItemEstoqueCommand, ItemEstoque>()
            .ForMember(dest => dest.UnidadeMedida, opt => opt.MapFrom(src =>
                Enum.Parse<UnidadeMedidaEnum>(src.UnidadeMedida, true)));
    }
}
