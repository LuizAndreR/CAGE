using FluentResults;
using MediatR;
using System.Text.Json.Serialization;

namespace CakeGestao.Application.Features.Empresas.UpdateStatus;

public class UpdateStatusEmpresaCommand : IRequest<Result>
{
    [JsonIgnore]
    public int EmpresaId { get; set; }

    public required string Status { get; set; }
}
