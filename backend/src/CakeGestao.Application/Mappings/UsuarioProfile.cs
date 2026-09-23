using AutoMapper;
using CakeGestao.Application.Features.User.Common;
using CakeGestao.Domain.Entities;

namespace CakeGestao.Application.Mappings;

public class UsuarioProfile : Profile
{
    public UsuarioProfile()
    {
        CreateMap<Usuario, UsuarioResponse>()
            .ForMember(dest => dest.DataInicio, opt => opt.MapFrom(src => src.DataCriacao));
    }
}
