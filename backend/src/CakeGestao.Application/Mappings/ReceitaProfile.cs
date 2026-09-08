using AutoMapper;
using CakeGestao.Application.Features.Receitas.Common;
using CakeGestao.Domain.Entities;

namespace CakeGestao.Application.Mappings;

public class ReceitaProfile : Profile
{
    public ReceitaProfile()
    {
        CreateMap<Receita, ReceitaResponseAll>();

        CreateMap<Receita, ReceitaResponse>()
            .ForMember(dest => dest.CustoExtra, opt => opt.MapFrom(src => src.PercentualCustoExtra));

        CreateMap<Ingrediente, IngredienteResponse>()
            .ForMember(dest => dest.Nome, opt => opt.MapFrom(src => src.Item.Nome))
            .ForMember(dest => dest.UnidadeMedida, opt => opt.MapFrom(src => src.UnidadeMedida.ToString()));
    }
}