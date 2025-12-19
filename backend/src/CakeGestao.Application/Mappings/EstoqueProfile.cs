using AutoMapper;
using CakeGestao.Application.Dtos.Requests.Estoque;
using CakeGestao.Domain.Entities;

namespace CakeGestao.Application.Mappings;

public class EstoqueProfile : Profile
{
    public EstoqueProfile()
    {
        CreateMap<CreateEstoqueRequest, ItemEstoque>()
            .ReverseMap();
    }
}
