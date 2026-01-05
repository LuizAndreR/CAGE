using CakeGestao.API.Extensions;
using CakeGestao.Application.Features.Estoque.Command.AddQuantidade;
using CakeGestao.Application.Features.Estoque.Command.Create;
using CakeGestao.Application.Features.Estoque.Command.Delete;
using CakeGestao.Application.Features.Estoque.Command.RemoverQuantidade;
using CakeGestao.Application.Features.Estoque.Command.Update;
using CakeGestao.Application.Features.Estoque.Query.Alerta;
using CakeGestao.Application.Features.Estoque.Query.GetAll;
using CakeGestao.Application.Features.Estoque.Query.GetItem;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CakeGestao.API.Controllers;

[ApiController]
[Authorize]
[Route("api/estoque/")]
public class EstoqueController : ApiControllerBase
{
    private readonly ILogger<EstoqueController> _logger;
    private readonly IMediator _mediator;

    public EstoqueController(IMediator mediator, ILogger<EstoqueController> logger)
    {
        _mediator = mediator;
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
        var listItemEstoqueResult = await _mediator.Send(new GetAllItemEstoqueQuery { EmpresaId = empresaId.Value});
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

        var itemEstoqueResult = await _mediator.Send(new GetItemEstoqueQuery { EmpresaId = empresaId.Value, ItemId = id });
        return HandleResult(itemEstoqueResult);
    }

    [HttpGet("alert")]
    public async Task<IActionResult> GetEstoqueAlertAsync()
    {
        _logger.LogInformation("Iniciando processo de obtenção de alertas de estoque.");
        var empresaId = User.GetEmpresaId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("Token de autorização inválido ou não contém EmpresaId.");
            return Unauthorized("Token inválido.");
        }

        var alertEstoqueResult = await _mediator.Send(new GetAlertaEstoqueQuery { EmpresaId = empresaId.Value });
        return HandleResult(alertEstoqueResult);
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateEstoque([FromBody]CreateEstoqueCommand request)
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
        var result = await _mediator.Send(request);
        return HandleResult<object>(result);
    }

    [HttpPatch("update/{id}")]
    public async Task<IActionResult> UpdateEstoque([FromRoute]int id, [FromBody] UpdateItemEstoqueCommand request)
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
        var result = await _mediator.Send(request);
        return HandleResult<object>(result);
    }

    [HttpPut("add-quantidade/{itemId}")]
    public async Task<IActionResult> AddQuantidadeEstoque([FromRoute]int itemId, [FromBody] AddQuantidadeEstoqueCommand request)
    {
        _logger.LogInformation("Iniciando processo de adição de quantidade ao estoque. EstoqueId: {EstoqueId}, QuantidadeAdicionar: {QuantidadeAdicionar}, Valor: {Valor}", itemId, request.QuantidadeAdicionar, request.Valor);

        var empresaId = User.GetEmpresaId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("Token de autorização inválido ou não contém EmpresaId.");
            return Unauthorized("Token inválido.");
        }
        request.EmpresaId = empresaId.Value;

        request.ItemId = itemId;
        var result = await _mediator.Send(request);
        return HandleResult<object>(result);
    }

    [HttpPut("remove-quantidade/{itemId}")]
    public async Task<IActionResult> RemoveQuantidadeEstoque([FromRoute]int itemId, [FromBody] RemoveQuantidadeEstoqueCommand request)
    {
        _logger.LogInformation("Iniciando processo de remoção de quantidade do estoque. EstoqueId: {EstoqueId}, QuantidadeRemover: {QuantidadeRemover}", itemId, request.QuantidadeARemover);
        var empresaId = User.GetEmpresaId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("Token de autorização inválido ou não contém EmpresaId.");
            return Unauthorized("Token inválido.");
        }
        request.EmpresaId = empresaId.Value;
        request.ItemId = itemId;
        var result = await _mediator.Send(request);
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
        var result = await  _mediator.Send(new DeleteItemEstoqueCommand { EmpresaId = empresaId.Value , ItemId = id});
        return HandleResult<object>(result);
    }
}
