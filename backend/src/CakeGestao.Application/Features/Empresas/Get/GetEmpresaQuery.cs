using CakeGestao.Application.Features.Empresas.Common;
using FluentResults;
using MediatR;

namespace CakeGestao.Application.Features.Empresas.Get;

public class GetEmpresaQuery : IRequest<Result<EmpresaResponse>>
{
    public int Id { get; set; }
}
