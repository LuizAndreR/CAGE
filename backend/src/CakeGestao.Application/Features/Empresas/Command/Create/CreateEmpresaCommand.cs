using FluentResults;
using MediatR;

namespace CakeGestao.Application.Dtos.Requests.Empresa;

public class CreateEmpresaCommand : IRequest<Result>
{
    public required string Nome { get; set; }
    public required string Endereco { get; set; }
}
