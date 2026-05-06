using CakeGestao.API.Extensions;
using CakeGestao.Application.Features.Pedidos.Command.Create;
using CakeGestao.Application.Features.Pedidos.Query.GetAll;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CakeGestao.API.Controllers;

[ApiController]
[Route("api/pedido")]
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

    [HttpGet("getall")]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("{LogPrefix} Recebida requisição para listar pedidos.", ControllerLogPrefix);
        
        var empresaId = User.GetEmpresaId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Falha de autorização: Token sem EmpresaId válido.", ControllerLogPrefix);
            return Unauthorized();
        }

        GetAllPedidoQuery request = new GetAllPedidoQuery{ EmpresaId = empresaId.Value};
        
        var pedidoResult = await _mediator.Send(request);
        return HandleResult(pedidoResult, _logger, ControllerLogPrefix);
    }
    
    [HttpPost("create")]
    public async Task<IActionResult> CreatePedido([FromBody] CreatePedidoCommand request)
    {
        _logger.LogInformation("{LogPrefix} Recebida requisição para criar um novo pedido.", ControllerLogPrefix);
        
        var empresaId = User.GetEmpresaId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Falha de autorização ao tentar criar pedido.", ControllerLogPrefix);
            return Unauthorized();
        }
        
        request.EmpresaId = empresaId.Value;
        
        var pedidoResult = await _mediator.Send(request);
        return HandleResult<object>(pedidoResult, _logger, ControllerLogPrefix);
    }
}