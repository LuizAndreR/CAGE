using CakeGestao.API.Extensions;
using CakeGestao.Application.Features.Receitas.Command.Create;
using CakeGestao.Application.Features.Receitas.Query.GetAll;
using CakeGestao.Application.Features.Receitas.Query.GetReceita;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Mvc;
namespace CakeGestao.API.Controllers;

[ApiController]
[Route("api/receita/")]
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

    [HttpGet("get/{id}")]
    public async Task<IActionResult> GetReceitaById([FromRoute]int id)
    {
        _logger.LogInformation("Recebendo requisição para busca de receita de ID: {Id}", id);

        var empresaId = User.GetEmpresaId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Falha de autorização ao buscar item {Id}.", ControllerLogPrefix, id);
            return Unauthorized("Token inválido.");
        }

        var result = await _mediator.Send(new GetReceitaQuery{Id = id});
        return HandleResult(result, _logger, ControllerLogPrefix);
    }

    [HttpGet("getall")]
    public async Task<IActionResult> GetAllReceitas()
    {
        _logger.LogInformation("Recebendo requisição para busca todas receita cadastradas com sucesso.");

        var empresaId = User.GetEmpresaId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Falha de autorização: Token sem EmpresaId válido.", ControllerLogPrefix);
            return Unauthorized("Token inválido.");
        }

        var result = await _mediator.Send(new GetAllReceitaQuery());
        return HandleResult(result, _logger, ControllerLogPrefix);
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateReceita([FromBody]CreateReceitaCommand request)
    {
        _logger.LogInformation("{LogPrefix} Criando novo item de estoque: {Nome}", ControllerLogPrefix, request.Nome);

        var empresaId = User.GetEmpresaId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Tentativa de criação sem EmpresaId válido. Item: {Nome}", ControllerLogPrefix, request.Nome);
            return Unauthorized("Token inválido.");
        }

        var result = await _mediator.Send(request);

        return HandleResult<object>(result, _logger, ControllerLogPrefix);
    }
}
