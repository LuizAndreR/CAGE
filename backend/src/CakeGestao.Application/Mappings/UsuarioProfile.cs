using AutoMapper;
using CakeGestao.Application.Dtos.Requests.Auth;
using CakeGestao.Application.Dtos.Responses;
using CakeGestao.Domain.Entities;
using CakeGestao.Infrastructure.Data.Repositories;

namespace CakeGestao.Application.Mappings;

public class UsuarioProfile : Profile
{
    public UsuarioProfile()
    {
        CreateMap<Usuario, CadastroRequest>()
            .ReverseMap();
        CreateMap<Usuario, UsuarioResponse>()
            .ReverseMap();
    }
}
