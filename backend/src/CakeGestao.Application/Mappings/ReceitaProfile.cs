using AutoMapper;
using CakeGestao.Application.Dtos.Requests.Receita;
using CakeGestao.Domain.Entities;

namespace CakeGestao.Application.Mappings;

public class ReceitaProfile : Profile
{
    public ReceitaProfile()
    {
        CreateMap<CreateReceitaRequest, Receita>()
            .ReverseMap();
    }
}