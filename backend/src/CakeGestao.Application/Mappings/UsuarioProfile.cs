using AutoMapper;
using CakeGestao.Application.Dtos.Responses;
using CakeGestao.Application.Features.Auth.Cadastro;
using CakeGestao.Domain.Entities;
using CakeGestao.Infrastructure.Data.Repositories;

namespace CakeGestao.Application.Mappings;

public class UsuarioProfile : Profile
{
    public UsuarioProfile()
    {
        CreateMap<CadastroCommand, Usuario>();
        CreateMap<Usuario, UsuarioResponse>();
    }
}
