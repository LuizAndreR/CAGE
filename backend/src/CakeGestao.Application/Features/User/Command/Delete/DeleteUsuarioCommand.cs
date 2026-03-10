using FluentResults;
using MediatR;

namespace CakeGestao.Application.Features.User.Command.Delete;

public class DeleteUsuarioCommand : IRequest<Result>
{
    public int EmpresaId { get; set; }
    public int Id { get; set; }
}