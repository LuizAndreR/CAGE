using CakeGestao.Application.Features.Empresas.Common;
using FluentResults;
using MediatR;

namespace CakeGestao.Application.Features.Empresas.Query.GetAll;

public class GetAllEmpresaQuery : IRequest<Result<List<EmpresaResponse>>>
{
    
}
