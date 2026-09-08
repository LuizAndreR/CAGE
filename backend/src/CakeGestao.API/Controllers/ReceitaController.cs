using CakeGestao.API.Extensions;
using CakeGestao.Application.Features.Receitas.Command.Create;
using CakeGestao.Application.Features.Receitas.Command.Delete;
using CakeGestao.Application.Features.Receitas.Command.Status;
using CakeGestao.Application.Features.Receitas.Command.Update;
using CakeGestao.Application.Features.Receitas.Query.GetAll;
using CakeGestao.Application.Features.Receitas.Query.GetReceita;
using MediatR;
using Microsoft.AspNetCore.Mvc;
namespace CakeGestao.API.Controllers;

[ApiController]
[Route("api/receita")]
public class ReceitaController : ApiControllerBase
{
    private readonly ILogger<ReceitaController> _logger;
    private readonly IMediator _mediator;
    private const string ControllerLogPrefix = "[Receita Controller]";

    public ReceitaController(ILogger<ReceitaController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetReceitaById([FromRoute]int id)
    {
        _logger.LogInformation("{LogPrefix} Recebida requisição para buscar receita. ID: {Id}", ControllerLogPrefix, id);

        var empresaId = User.GetEmpresaId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Falha de autorização ao buscar receita {Id}.", ControllerLogPrefix, id); 
            return Unauthorized("Token inválido.");
        }

        var result = await _mediator.Send(new GetReceitaQuery{Id = id, EmpresaId = empresaId.Value});
        return HandleResult(result, _logger, ControllerLogPrefix);
    }

    [HttpGet("")]
    public async Task<IActionResult> GetAllReceitas()
    {
        _logger.LogInformation("{LogPrefix} Recebida requisição para listar receitas da empresa.", ControllerLogPrefix);

        var empresaId = User.GetEmpresaId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Falha de autorização: Token sem EmpresaId válido.", ControllerLogPrefix);
            return Unauthorized("Token inválido.");
        }

        var result = await _mediator.Send(new GetAllReceitaQuery { EmpresaId = empresaId.Value});
        return HandleResult(result, _logger, ControllerLogPrefix);
    }

    [HttpPost("")]
    public async Task<IActionResult> CreateReceita([FromBody]CreateReceitaCommand request)
    {
        _logger.LogInformation("{LogPrefix} Criando uma nova receita chamada: {Nome}", ControllerLogPrefix, request.Nome);

        var empresaId = User.GetEmpresaId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Tentativa de criação sem EmpresaId válido. Receita: {Nome}", ControllerLogPrefix, request.Nome);
            return Unauthorized("Token inválido.");
        }

        request.EmpresaId = empresaId.Value;
        
        var result = await _mediator.Send(request);

        return HandleResult<object>(result, _logger, ControllerLogPrefix);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReceita([FromRoute]int id, [FromBody] UpdateReceitaCommand request)
    {
        _logger.LogInformation("{LogPrefix} Atualizando receita ID: {Id}", ControllerLogPrefix, id);
        var empresaId = User.GetEmpresaId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Tentativa de atualização sem EmpresaId válido. Receita ID: {Id}", ControllerLogPrefix, id);
            return Unauthorized("Token inválido.");
        }
        request.EmpresaId = empresaId.Value;
        request.Id = id;

        var result = await _mediator.Send(request); 
        return HandleResult<object>(result, _logger, ControllerLogPrefix);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus([FromRoute] int id, [FromBody] UpdateReceitaStatusCommand request)
    {
        _logger.LogInformation("{LogPrefix} Atualizando status da receita ID: {Id}", ControllerLogPrefix, id);
        var empresaId = User.GetEmpresaId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Tentativa de atualização de status sem EmpresaId válido. Receita ID: {Id}", ControllerLogPrefix, id);
            return Unauthorized("Token inválido");
        }

        request.EmpresaId = empresaId.Value;
        request.ReceitaId = id;
        
        var result = await _mediator.Send(request);
        return HandleResult<object>(result, _logger, ControllerLogPrefix);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReceita([FromRoute] int id)
    {
        _logger.LogInformation("{LogPrefix} Delete status da receita ID: {Id}", ControllerLogPrefix, id);
        var empresaId = User.GetEmpresaId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Tentativa de atualização de status sem EmpresaId válido. Receita ID: {Id}", ControllerLogPrefix, id);
            return Unauthorized("Token inválido");
        }
        
        var request = new DeleteReceitaCommand{EmpresaId = empresaId.Value, ReceitaId = id};
        
        var result = await _mediator.Send(request);
        return HandleResult<object>(result, _logger, ControllerLogPrefix);
    }
}
