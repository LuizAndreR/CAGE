using CakeGestao.Application.Features.Receitas.Command.Create;
using CakeGestao.Application.Features.Receitas.Query.GetAll;
using CakeGestao.Application.Features.Receitas.Query.GetReceita;
using MediatR;
using Microsoft.AspNetCore.Mvc;
namespace CakeGestao.API.Controllers;

[ApiController]
[Route("api/receita/")]
public class ReceitaController : ApiControllerBase
{
    private readonly ILogger<ReceitaController> _logger;
    private readonly IMediator _mediator;

    public ReceitaController(ILogger<ReceitaController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [HttpGet("get/{id}")]
    public async Task<IActionResult> GetReceitaById([FromRoute]int id)
    {
        _logger.LogInformation("Recebendo requisição para busca de receita de ID: {Id}", id);
        var receitaResult = await _mediator.Send(new GetReceitaQuery{Id = id});
        _logger.LogInformation("Receita de ID: {Id} encontrada com sucesso. Código HTTP 200.", id);
        return HandleResult(receitaResult);
    }

    [HttpGet("getall")]
    public async Task<IActionResult> GetAllReceitas()
    {
        _logger.LogInformation("Recebendo requisição para busca todas receita cadastradas com sucesso.");
        var receitaResult = await _mediator.Send(new GetAllReceitaQuery());
        _logger.LogInformation("Foi encontrado com susseso {Numero} receitas cadastrada no banco de dados", receitaResult.Value.Count);
        return HandleResult(receitaResult);
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateReceita([FromBody]CreateReceitaCommand request)
    {
        _logger.LogInformation("Recebendo requisição para criação de receita de nome: {Nome}", request.Nome);

        var result = await _mediator.Send(request);

        _logger.LogInformation("Receita {Nome} criada com sucesso. Código HTTP 201.", request.Nome);
        return HandleResult<object>(result);
    }
}
