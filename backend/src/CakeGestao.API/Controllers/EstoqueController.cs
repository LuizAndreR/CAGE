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

    [HttpGet("getall")]
    public async Task<IActionResult> GetAllEstoqueAsync()
    {
        _logger.LogInformation("");
        var empresaId = User.GetEmpresaId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("Token de autorização inválido ou não contém EmpresaId.");
            return Unauthorized("Token inválido.");
        }
        var listItemEstoqueResult = await _estoqueService.GetAllItemEstoque(new ItemEstoqueRequest { EmpresaId = empresaId.Value});
        return HandleResult(listItemEstoqueResult);
    }

    [HttpGet("get/{id}")]
    public async Task<IActionResult> GetEstoqueByIdAsync([FromRoute]int id)
    {
        _logger.LogInformation("Iniciando processo de obtenção de item de estoque por ID: {Id}", id);
        var empresaId = User.GetEmpresaId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("Token de autorização inválido ou não contém EmpresaId.");
            return Unauthorized("Token inválido.");
        }

        var itemEstoqueResult = await _estoqueService.GetItemEstoqueById(new ItemEstoqueRequest
        {
            EmpresaId = empresaId.Value,
            ItemId = id
        });
        return HandleResult(itemEstoqueResult);
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

    [HttpPatch("update/{id}")]
    public async Task<IActionResult> UpdateEstoque([FromRoute]int id, [FromBody] UpdateItemEstoqueRequest request)
    {
        _logger.LogInformation("Iniciando processo de atualização de item de estoque. ItemId: {ItemId}, NovoNome: {NovoNome}", id, request.Nome);
        var empresaId = User.GetEmpresaId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("Token de autorização inválido ou não contém EmpresaId.");
            return Unauthorized("Token inválido.");
        }
        request.EmpresaId = empresaId.Value;
        request.ItemId = id;
        var result = await _estoqueService.UpdateItemEstoqueAsync(request);
        return HandleResult<object>(result);
    }

    [HttpPut("add-quantidade/{EstoqueId}")]
    public async Task<IActionResult> AddQuantidadeEstoque([FromRoute]int EstoqueId, [FromBody] AddQuantidadeEstoqueRequest request)
    {
        _logger.LogInformation("Iniciando processo de adição de quantidade ao estoque. EstoqueId: {EstoqueId}, QuantidadeAdicionar: {QuantidadeAdicionar}, Valor: {Valor}", EstoqueId, request.QuantidadeAdicionar, request.Valor);

        var empresaId = User.GetEmpresaId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("Token de autorização inválido ou não contém EmpresaId.");
            return Unauthorized("Token inválido.");
        }
        request.EmpresaId = empresaId.Value;

        request.EstoqueId = EstoqueId;
        var result = await _estoqueService.AddQuantidadeEstoqueAsync(request);
        return HandleResult<object>(result);
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteItemEstoque([FromRoute] int id)
    {
        _logger.LogInformation("Iniciando processo de deleção de item de estoque. ItemId: {ItemId}", id);
        var empresaId = User.GetEmpresaId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("Token de autorização inválido ou não contém EmpresaId.");
            return Unauthorized("Token inválido.");
        }
        var result = await _estoqueService.DeleteItemEstoqueAsync(new ItemEstoqueRequest { EmpresaId = empresaId.Value , ItemId = id});
        return HandleResult<object>(result);
    }
}
