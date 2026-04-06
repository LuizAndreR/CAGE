using AutoMapper;
using CakeGestao.Application.Features.Receitas.Common;
using CakeGestao.Domain.Entities;

namespace CakeGestao.Application.Mappings;

public class ReceitaProfile : Profile
{
    public ReceitaProfile()
    {
        CreateMap<Receita, ReceitaResponse>();
        CreateMap<Ingrediente, IngredienteResponse>();

        CreateMap<Receita, ReceitaResponseAll>();
    }
}