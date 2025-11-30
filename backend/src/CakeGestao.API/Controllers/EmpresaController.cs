using CakeGestao.Application.Dtos.Requests.Empresa;
using CakeGestao.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CakeGestao.API.Controllers;

[ApiController]
[Route("api/empresa/")]
[Authorize(Roles = "Admin")]
public class EmpresaController : ApiControllerBase
{
    private readonly IEmpresaService _empresaService;
    private readonly ILogger<EmpresaController> _logger;

    public EmpresaController(IEmpresaService empresaService, ILogger<EmpresaController> logger)
    {
        _empresaService = empresaService;
        _logger = logger;
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAllEmpresas()
    {
        _logger.LogInformation("Recebendo solicitação de getall das empresa cadastrado no banco de dados");
        var result = await _empresaService.GetAllAsync();
        _logger.LogInformation("Solicitação de getall das empresa cadastrado no banco de dados processada com sucesso");
        return HandleResult(result);
    }

    [HttpGet("Get/{id}")]
    public async Task<IActionResult> GetEmpresaById(int id)
    {
        _logger.LogInformation("Recebendo solicitação para obter empresa com ID: {Id}", id);
        var result = await _empresaService.GetByIdAsync(id);
        _logger.LogInformation("Solicitação para obter empresa com ID: {Id} processada com sucesso", id);
        return HandleResult(result);
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateEmpresa([FromBody] CreateEmpresaRequest request)
    {
        _logger.LogInformation("Recebendo solicitação para criar uma nova empresa com o nome: {Nome}", request.Nome);
        var result = await _empresaService.CreateAsync(request);
        _logger.LogInformation("Solicitação para criar uma nova empresa com o nome: {Nome} processada com sucesso", request.Nome);
        return HandleResult<object>(result);
    }

    [HttpPatch("update/{id}")]
    public async Task<IActionResult> UpdateEmpresa([FromBody] UpdateEmpresaRequest request, [FromRoute] int id)
    {
        _logger.LogInformation("Recebendo solicitação para update da empresa com o id: {Id}", id);
        var result = await _empresaService.UpdateAsync(request, id);
        _logger.LogInformation("Solicitação para update da empresa com o id: {Id} processada com sucesso", id);
        return HandleResult<object> (result);
    }

    [HttpPut("updatestatus/{id}")]
    public async Task<IActionResult> UpdateStatusEmpresa([FromBody] UpdateStatusEmpresaRequest request, [FromRoute] int id)
    {
        _logger.LogInformation("Recebendo solicitação para update no status da empresa com o id: {Id}", id);
        var result = await _empresaService.UpdateStatusAsync(request, id);
        _logger.LogInformation("Solicitação para update status da empresa com o id: {Id} processada com sucesso", id);
        return HandleResult<object>(result);
    }

    [HttpDelete("delete/{Id}")]
    public async Task<IActionResult> DeleteEmpresa([FromRoute] int id)
    {
        _logger.LogInformation("Recebendo solicitação para delete da empresa com o id: {Id}", id);
        var result = await _empresaService.DeleteAsync(id);
        _logger.LogInformation("Solicitação para delete da empresa com o id: {Id} processada com sucesso", id);
        return HandleResult<object>(result);
    }
}
