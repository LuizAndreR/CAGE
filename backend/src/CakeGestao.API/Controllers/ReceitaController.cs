using CakeGestao.Application.Dtos.Requests.Receita;
using CakeGestao.Application.Services.Interface;
using Microsoft.AspNetCore.Mvc;
namespace CakeGestao.API.Controllers;

[ApiController]
[Route("api/receita/")]
public class ReceitaController : ControllerBase
{
    private readonly ILogger<ReceitaController> _logger;
    private readonly IReceitaService _receitaService;

    public ReceitaController(ILogger<ReceitaController> logger, IReceitaService receitaService)
    {
        _logger = logger;
        _receitaService = receitaService;
    }

    [HttpGet("get/{id}")]
    public async Task<IActionResult> GetReceitaById([FromRoute]int id)
    {
        _logger.LogInformation("Recebendo requisição para busca de receita de ID: {Id}", id);
        var receitaResponse = await _receitaService.GetReceita(id);
        _logger.LogInformation("Receita de ID: {Id} encontrada com sucesso. Código HTTP 200.", id);
        return Ok(receitaResponse.Value);
    }

    [HttpGet("getall")]
    public async Task<IActionResult> GetAllReceitas()
    {
        _logger.LogInformation("Recebendo requisição para busca todas receita cadastradas com sucesso.");
        var receitaResult = await _receitaService.GetAllReceita();
        _logger.LogInformation("Foi encontrado com susseso {Numero} receitas cadastrada no banco de dados", receitaResult.Value.Count);
        return Ok(receitaResult.Value);
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateReceita([FromBody]CreateReceitaRequest request)
    {
        _logger.LogInformation("Recebendo requisição para criação de receita de nome: {Nome}", request.Nome);

        var result = await _receitaService.CreateReceita(request);

        _logger.LogInformation("Receita {Nome} criada com sucesso. Código HTTP 201.", request.Nome);
        return StatusCode(StatusCodes.Status201Created);
    }
}
