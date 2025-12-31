using FluentResults;
using MediatR;

namespace CakeGestao.Application.Features.User.Command.UpdateFuncionario;

public class UpdateFuncionarioCommand : IRequest<Result>
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required string Role { get; set; }
}