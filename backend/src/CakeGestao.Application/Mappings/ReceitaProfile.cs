using AutoMapper;
using CakeGestao.Application.Dtos.Requests.Receita;
using CakeGestao.Application.Dtos.Responses;
using CakeGestao.Domain.Entities;

namespace CakeGestao.Application.Mappings;

public class ReceitaProfile : Profile
{
    public ReceitaProfile()
    {
        CreateMap<CreateReceitaRequest, Receita>()
            .ReverseMap();
        CreateMap<Receita, ReceitaResponse>()
            .ReverseMap();
    }
}