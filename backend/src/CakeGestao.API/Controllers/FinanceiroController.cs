using CakeGestao.API.Extensions;
using CakeGestao.Application.Features.Financeiro.Command.Cancelamento;
using CakeGestao.Application.Features.Financeiro.Command.Create;
using CakeGestao.Application.Features.Financeiro.Query.Get;
using CakeGestao.Application.Features.Financeiro.Query.GetAll;
using CakeGestao.Application.Features.Financeiro.Query.GetEntrada;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CakeGestao.API.Controllers;

[ApiController]
[Route("api/financeiro")]
[Authorize(Roles = "Admin, Dono")]
public class FinanceiroController : ApiControllerBase
{
    private readonly ILogger<FinanceiroController> _logger;
    private readonly IMediator _mediator;
    private const string ControllerLogPrefix = "[Financeiro Controller]";

    public FinanceiroController(ILogger<FinanceiroController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [HttpGet("")]
    public async Task<IActionResult> GetAll([FromQuery]string? tipo, [FromQuery]string? categoria, [FromQuery]int? mes, [FromQuery]int? ano)
    {
        _logger.LogInformation("{LogPrefix} Solicitando extrato financeiro completo.", ControllerLogPrefix);

        var empresaId = User.GetEmpresaId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Falha de autorização ao buscar extrato.", ControllerLogPrefix);
            return Unauthorized("Token inválido.");
        }
        
        var transacaoResult = await _mediator.Send(new GetAllTransacaoQuery { EmpresaId = empresaId.Value, Tipo = tipo, Categoria = categoria, Mes = mes, Ano = ano});
        return HandleResult(transacaoResult, _logger, ControllerLogPrefix);
    }

    [HttpGet("resumo")]
    public async Task<IActionResult> GetResumo()
    {
        _logger.LogInformation("{LogPrefix} Solicitando resumo financeiro.", ControllerLogPrefix);
        var empresaId = User.GetEmpresaId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Falha de autorização ao buscar resumo de entradas.", ControllerLogPrefix);
            return Unauthorized("Token inválido.");
        }
        var transacaoResult = await _mediator.Send(new GetFinanceiroResumoQuery { EmpresaId = empresaId.Value });

        return HandleResult(transacaoResult, _logger, ControllerLogPrefix);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        _logger.LogInformation("{LogPrefix} Buscando detalhes da transação. ID: {Id}", ControllerLogPrefix, id);

        var empresaId = User.GetEmpresaId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Falha de autorização ao buscar transação {Id}.", ControllerLogPrefix, id);
            return Unauthorized("Token inválido.");
        }

        var transacaoResult = await _mediator.Send(new GetTransacaoQuery { Id = id, EmpresaId = empresaId.Value });
        return HandleResult(transacaoResult, _logger, ControllerLogPrefix);
    }

    [HttpPost("")]
    public async Task<IActionResult> CreateTransacao([FromBody] CreateTransacaoCommand request)
    {
        _logger.LogInformation("{LogPrefix} Registrando nova movimentação financeira. Valor: {Valor} | Tipo: {Tipo}",
            ControllerLogPrefix, request.Valor, request.Tipo);

        var empresaId = User.GetEmpresaId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Tentativa de registro financeiro sem autorização válida.", ControllerLogPrefix);
            return Unauthorized("Token inválido.");
        }

        request.EmpresaId = empresaId.Value;
        var transacaoResult = await _mediator.Send(request);

        return HandleResult<object>(transacaoResult, _logger, ControllerLogPrefix);
    }

    [HttpPost("{id}/cancelamento")]
    public async Task<IActionResult> CancelarTransacao([FromBody] CancelTransacaoCommand request, [FromRoute] int id)
    {
        _logger.LogInformation("{LogPrefix} Solicitando cancelamento de transação. ID: {Id}", ControllerLogPrefix, id);
        var empresaId = User.GetEmpresaId();
        var userId = User.GetUserId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Tentativa de cancelamento sem autorização válida. ID: {Id}", ControllerLogPrefix, id);
            return Unauthorized("Token inválido.");
        }

        request.TransacaoId = id;
        request.EmpresaId = empresaId.Value;
        request.UsuarioId = userId.Value;
        var cancelResult = await _mediator.Send(request);

        return HandleResult<object>(cancelResult, _logger, ControllerLogPrefix);
    }
}