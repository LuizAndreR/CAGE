using FluentResults;
using MediatR;

namespace CakeGestao.Application.Features.Empresas.Delete;

public class DeleteEmpresaCommand : IRequest<Result>
{
    public int Id { get; set; }
}
