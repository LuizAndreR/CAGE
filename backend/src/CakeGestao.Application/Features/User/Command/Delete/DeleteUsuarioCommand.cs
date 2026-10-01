using FluentResults;
using MediatR;

namespace CakeGestao.Application.Features.User.Command.Delete;

public class DeleteUsuarioCommand : IRequest<Result>
{
    [System.Text.Json.Serialization.JsonIgnore]
    public bool AdminRole { get; set; }

    public int EmpresaId { get; set; }
    public int Id { get; set; }
}