using AutoMapper;
using CakeGestao.Application.UseCases.Estoque.Common;
using CakeGestao.Domain.Entities;

namespace CakeGestao.Application.Mappings;

public class EstoqueProfile : Profile
{
    public EstoqueProfile()
    {
        CreateMap<ItemEstoque, ItemEstoqueResponse>();
    }
}
