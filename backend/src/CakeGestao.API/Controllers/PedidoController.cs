using CakeGestao.API.Extensions;
using CakeGestao.Application.Features.Pedidos.Command.Create;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CakeGestao.API.Controllers;

[ApiController]
[Route("api/pedido/")]
[Authorize(Roles = "Admin, Dono")]
public class PedidoController : ApiControllerBase
{
    private readonly ILogger<FinanceiroController> _logger;
    private readonly IMediator _mediator;
    private const string ControllerLogPrefix = "[Pedido Controller]";
    
    public PedidoController(ILogger<FinanceiroController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreatePedido([FromBody] CreatePedidoCommand request)
    {
        _logger.LogInformation("");

        var empresaId = User.GetEmpresaId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Tentativa de registro financeiro sem autorização válida.", ControllerLogPrefix);
            return Unauthorized("Token inválido.");
        }
        
        request.EmpresaId = empresaId.Value;
        
        var pedidoResult = await _mediator.Send(request);
        return HandleResult<object>(pedidoResult, _logger, ControllerLogPrefix);
    }
}