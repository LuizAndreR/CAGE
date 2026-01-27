using CakeGestao.Application.Features.User.Common;
using FluentResults;
using MediatR;

namespace CakeGestao.Application.Features.User.Query.GetFuncionario;

public class GetFuncionariosQuery : IRequest<Result<List<UsuarioResponse>>>
{
    public int EmpresaId { get; set; }
}
