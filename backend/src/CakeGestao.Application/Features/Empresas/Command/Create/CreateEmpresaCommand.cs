using FluentResults;
using MediatR;

namespace CakeGestao.Application.Features.Empresas.Command.Create;

public class CreateEmpresaCommand : IRequest<Result>
{
    public required string Nome { get; set; }
    public required string Endereco { get; set; }
}
