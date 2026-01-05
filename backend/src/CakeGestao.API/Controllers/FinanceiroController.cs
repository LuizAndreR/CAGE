using CakeGestao.API.Extensions;
using CakeGestao.Application.Features.Financeiro.Create;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CakeGestao.API.Controllers;

[ApiController]
[Route("api/financeiro/")]
[Authorize(Roles = "Admin, Dono")]
public class FinanceiroController : ApiControllerBase
{
    private readonly ILogger<FinanceiroController> _logger;
    private readonly IMediator _mediator;

    public FinanceiroController(ILogger<FinanceiroController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [HttpPost("Create")]
    public async Task<IActionResult> CreateTransacao([FromBody] CreateTransacaoCommand request)
    {
        var empresaId = User.GetEmpresaId();
        _logger.LogInformation("Recebendo requisição para create uma nao transação de valor {Valor} de tipo {Tipo} da empresa {EmpresaId}", request.Valor, request.Tipo, empresaId.Value);
        request.EmpresaId = empresaId.Value;
        var transacaoResult = await _mediator.Send(request);
        return HandleResult<object>(transacaoResult);
    }
}
