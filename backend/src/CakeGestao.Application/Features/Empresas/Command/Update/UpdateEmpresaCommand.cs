using FluentResults;
using MediatR;
using System.Text.Json.Serialization;

namespace CakeGestao.Application.Features.Empresas.Command.Update;

public class UpdateEmpresaCommand : IRequest<Result>
{
    [JsonIgnore]
    public int Id { get; set; }

    public required string Nome { get; set; }
    public required string Endereco { get; set; }
    public required string Status { get; set; }
}
