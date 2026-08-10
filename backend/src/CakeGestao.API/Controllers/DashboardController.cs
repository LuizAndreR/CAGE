using CakeGestao.API.Extensions;
using CakeGestao.Application.Features.Dashboard.Query;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CakeGestao.API.Controllers;

[ApiController]
[Route("api/dashboard/")]
[Authorize(Roles = "Admin, Dono")]
public class DashboardController : ApiControllerBase
{
    private readonly ILogger<DashboardController> _logger;
    private readonly IMediator _mediator;
    private const string ControllerLogPrefix = "[Dashboard Controller]";

    public DashboardController(ILogger<DashboardController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [HttpGet("resumo")]
    public async Task<IActionResult> GetResumoAsync()
    {
        var empresaId = User.GetEmpresaId();
        var nomeUsuarioClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? User.FindFirst("Nome")?.Value ?? "Usuário";

        var result = await _mediator.Send(new GetDashboardResumoQuery { EmpresaId = empresaId.Value , NomeUsuario = nomeUsuarioClaim });

        return HandleResult(result, _logger, ControllerLogPrefix);
    }
}