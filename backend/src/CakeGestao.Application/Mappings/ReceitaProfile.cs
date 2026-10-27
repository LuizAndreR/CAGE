using AutoMapper;
using CakeGestao.Application.Features.Receitas.Common;
using CakeGestao.Domain.Entities;

namespace CakeGestao.Application.Mappings;

public class ReceitaProfile : Profile
{
    public ReceitaProfile()
    {
        CreateMap<Receita, ReceitaResponseAll>();

        CreateMap<Receita, ReceitaResponse>();

        CreateMap<IngredienteAlternativa, AlternativaIngredienteResponse>()
            .ForMember(dest => dest.Nome, opt => opt.MapFrom(src => src.Item.Nome))
            .ForMember(dest => dest.Marca, opt => opt.MapFrom(src => src.Item.Marca))
            .ForMember(dest => dest.UnidadeMedida, opt => opt.MapFrom(src => src.Item.UnidadeMedida.ToString()));

        CreateMap<Ingrediente, IngredienteResponse>()
            .ForMember(dest => dest.Alternativas, opt => opt.MapFrom(src => src.Alternativas.OrderBy(a => a.Ordem)))
            .ForMember(dest => dest.Marca, opt => opt.MapFrom(src => src.Item.Marca))
            .ForMember(dest => dest.AlternativasIds, opt => opt.MapFrom(src => src.Alternativas.OrderBy(a => a.Ordem).Select(a => a.ItemId)))
            .ForMember(dest => dest.Nome, opt => opt.MapFrom(src => src.Item.Nome))
            .ForMember(dest => dest.UnidadeMedida, opt => opt.MapFrom(src => src.UnidadeMedida.ToString()));
    }
}