using CakeGestao.Application.Dtos.Requests.Empresa;
using CakeGestao.Application.Services.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CakeGestao.API.Controllers;

[ApiController]
[Route("api/empresa")]
[Authorize(Roles = "Admin")]
public class EmpresaController : ApiControllerBase
{
    private readonly EmpresaService _empresaService;
    private readonly ILogger<EmpresaController> _logger;

    public EmpresaController(EmpresaService empresaService, ILogger<EmpresaController> logger)
    {
        _empresaService = empresaService;
        _logger = logger;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateEmpresa([FromBody] CreateEmpresaRequest request)
    {
        _logger.LogInformation("Recebendo solicitação para criar uma nova empresa com o nome: {Nome}", request.Nome);
        var result = await _empresaService.CreateAsync(request);
        return HandleResult<object>(result);
    }
}
