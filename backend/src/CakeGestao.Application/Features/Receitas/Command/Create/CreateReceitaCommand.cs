using FluentResults;
using MediatR;

namespace CakeGestao.Application.Features.Receitas.Command.Create;

public class CreateReceitaCommand : IRequest<Result>
{
    public required string Nome { get; set; }
    public required string ModoPreparo { get; set; }
    public decimal PrecoVenda { get; set; }
}