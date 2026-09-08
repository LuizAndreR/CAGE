using CakeGestao.Application.Features.User.Common;
using FluentResults;
using MediatR;

namespace CakeGestao.Application.Features.User.Query.Get;

public class GetUsuarioQuery : IRequest<Result<UsuarioResponse>>
{
    public int EmpresaId { get; set; }
    public int Id { get; set; }
}