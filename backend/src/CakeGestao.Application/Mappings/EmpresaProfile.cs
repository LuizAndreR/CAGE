using AutoMapper;
using CakeGestao.Application.Dtos.Requests.Empresa;
using CakeGestao.Application.Dtos.Responses;
using CakeGestao.Domain.Entities;

namespace CakeGestao.Application.Mappings;

public class EmpresaProfile : Profile
{
    public EmpresaProfile()
    {
        CreateMap<CreateEmpresaRequest, Empresa>()
            .ReverseMap();
        CreateMap<Empresa, EmpresaResponse>()
            .ReverseMap();
    }
}
