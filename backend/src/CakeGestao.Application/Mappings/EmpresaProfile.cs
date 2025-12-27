using AutoMapper;
using CakeGestao.Application.Features.Empresas.Common;
using CakeGestao.Domain.Entities;

namespace CakeGestao.Application.Mappings;

public class EmpresaProfile : Profile
{
    public EmpresaProfile()
    {
        CreateMap<Empresa, EmpresaResponse>()
            .ReverseMap();
    }
}
