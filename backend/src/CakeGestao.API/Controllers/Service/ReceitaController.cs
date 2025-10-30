using CakeGestao.Application.Dtos.Requests.Receita;
using CakeGestao.Application.Services.Interface;
using CakeGestao.Application.Services.Service;
using Microsoft.AspNetCore.Mvc;
namespace CakeGestao.API.Controllers.Service;

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

    [HttpPost("create")]
    public async Task<IActionResult> CreateReceita([FromBody]CreateReceitaRequest request)
    {
        _logger.LogInformation("Recebendo requisição para criação de receita de nome: {Nome}", request.Nome);

        var result = await _receitaService.CreateReceita(request);

        _logger.LogInformation("Receita {Nome} criada com sucesso. Código HTTP 201.", request.Nome);
        return StatusCode(StatusCodes.Status201Created);
    }
}
