using AutoMapper;
using CakeGestao.Application.Dtos.Requests.Empresa;
using CakeGestao.Application.Features.Empresas.Common;
using CakeGestao.Domain.Entities;

namespace CakeGestao.Application.Mappings;

public class EmpresaProfile : Profile
{
    public EmpresaProfile()
    {
        CreateMap<CreateEmpresaCommand, Empresa>()
            .ReverseMap();
        CreateMap<Empresa, EmpresaResponse>()
            .ReverseMap();
    }
}
