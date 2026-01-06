using AutoMapper;
using CakeGestao.Application.Features.Financeiro.Common;
using CakeGestao.Domain.Entities;

namespace CakeGestao.Application.Mappings;

public class TransecaoProfile : Profile
{
    public TransecaoProfile()
    {
        CreateMap<TransacaoFinanceira, TransacaoResponse>();
    }
}
