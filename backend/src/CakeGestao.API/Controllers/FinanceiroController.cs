using CakeGestao.API.Extensions;
using CakeGestao.Application.Dtos.Requests.Transacao;
using CakeGestao.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CakeGestao.API.Controllers;

[ApiController]
[Route("api/financeiro/")]
[Authorize(Roles = "Admin, Dono")]
public class FinanceiroController : ApiControllerBase
{
    private readonly ILogger<FinanceiroController> _logger;
    private readonly IFinanceiroService _financeiroService;

    public FinanceiroController(ILogger<FinanceiroController> logger, IFinanceiroService financeiroService)
    {
        _logger = logger;
        _financeiroService = financeiroService;
    }

    [HttpPost("Create")]
    public async Task<IActionResult> CreateTransacao([FromBody] CreateTransacaoRequest request, [FromQuery] int? pedidoId)
    {
        var empresaId = User.GetEmpresaId();
        _logger.LogInformation("Recebendo requisição para create uma nao transação de valor {Valor} de tipo {Tipo} da empresa {EmpresaId}", request.Valor, request.Tipo, empresaId.Value);
        
        var transacaoResult = await _financeiroService.CreateTransacaoAsync(request, empresaId.Value, pedidoId);
        return HandleResult<object>(transacaoResult);
    }
}
