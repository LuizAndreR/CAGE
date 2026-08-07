using System.Text.Json.Serialization;
using CakeGestao.Application.Features.Dashboard.Common;
using FluentResults;
using MediatR;

namespace CakeGestao.Application.Features.Dashboard.Query;

public class GetDashboardResumoQuery : IRequest<Result<DashboardResponseDto>>
{
    [JsonIgnore]
    public int EmpresaId { get; set; }
}