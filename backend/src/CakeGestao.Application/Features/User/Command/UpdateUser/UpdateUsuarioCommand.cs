using System.Text.Json.Serialization;
using FluentResults;
using MediatR;

namespace CakeGestao.Application.Features.User.Command.UpdateUser;

public class UpdateUsuarioCommand : IRequest<Result>
{
    [JsonIgnore]
    public int Id { get; set; }

    public required string Nome { get; set; }
    public required string Email { get; set; }
}