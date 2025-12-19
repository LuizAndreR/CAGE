using CakeGestao.API.Extensions;
using CakeGestao.Application.Dtos.Requests.Estoque;
using CakeGestao.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CakeGestao.API.Controllers;

[ApiController]
[Authorize]
[Route("api/estoque/")]
public class EstoqueController : ApiControllerBase
{
    private readonly ILogger<EstoqueController> _logger;
    private readonly IEstoqueService _estoqueService;

    public EstoqueController(IEstoqueService estoqueService, ILogger<EstoqueController> logger)
    {
        _estoqueService = estoqueService;
        _logger = logger;
    }

    
    [HttpPost("create")]
    public async Task<IActionResult> CreateEstoque([FromBody]CreateEstoqueRequest request)
    {
        _logger.LogInformation("Iniciando processo de criação de novo item de estoque.");
        var empresaId = User.GetEmpresaId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("Token de autorização inválido ou não contém EmpresaId.");
            return Unauthorized("Token inválido.");
        }
        request.EmpresaId = empresaId.Value;

        _logger.LogInformation("Recebendo requisição para criação de novo item de estoque. EmpresaId: {EmpresaId}, Nome: {Nome}", request.EmpresaId, request.Nome);
        var result = await _estoqueService.CreateEstoqueAsync(request);
        return HandleResult<object>(result);
    }
}
