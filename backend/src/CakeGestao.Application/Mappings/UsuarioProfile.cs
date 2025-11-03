using AutoMapper;
using CakeGestao.Application.Dtos.Requests.Auth;
using CakeGestao.Domain.Entities;

namespace CakeGestao.Application.Mappings;

public class UsuarioProfile : Profile
{
    public UsuarioProfile()
    {
        CreateMap<Usuario, CadastroRequest>()
            .ReverseMap();
    }
}
