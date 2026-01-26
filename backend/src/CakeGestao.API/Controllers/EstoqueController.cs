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
    private const string ControllerLogPrefix = "[Estoque Controller]";

    public EstoqueController(IMediator mediator, ILogger<EstoqueController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("getall")]
    public async Task<IActionResult> GetAllEstoqueAsync()
    {
        _logger.LogInformation("{LogPrefix} Solicitando listagem geral de estoque.", ControllerLogPrefix);

        var empresaId = User.GetEmpresaId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Falha de autorização: Token sem EmpresaId válido.", ControllerLogPrefix);
            return Unauthorized("Token inválido.");
        }

        var result = await _mediator.Send(new GetAllItemEstoqueQuery { EmpresaId = empresaId.Value });
        return HandleResult(result);
    }

    [HttpGet("get/{id}")]
    public async Task<IActionResult> GetEstoqueByIdAsync([FromRoute] int id)
    {
        _logger.LogInformation("{LogPrefix} Buscando item de estoque. ID: {Id}", ControllerLogPrefix, id);

        var empresaId = User.GetEmpresaId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Falha de autorização ao buscar item {Id}.", ControllerLogPrefix, id);
            return Unauthorized("Token inválido.");
        }

        var result = await _mediator.Send(new GetItemEstoqueQuery { EmpresaId = empresaId.Value, ItemId = id });
        return HandleResult(result);
    }

    [HttpGet("alert")]
    public async Task<IActionResult> GetEstoqueAlertAsync()
    {
        _logger.LogInformation("{LogPrefix} Verificando alertas de estoque baixo/validade.", ControllerLogPrefix);

        var empresaId = User.GetEmpresaId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Falha de autorização ao buscar alertas.", ControllerLogPrefix);
            return Unauthorized("Token inválido.");
        }

        var result = await _mediator.Send(new GetAlertaEstoqueQuery { EmpresaId = empresaId.Value });
        return HandleResult(result);
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateEstoque([FromBody] CreateEstoqueCommand request)
    {
        _logger.LogInformation("{LogPrefix} Criando novo item de estoque: {Nome}", ControllerLogPrefix, request.Nome);

        var empresaId = User.GetEmpresaId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Tentativa de criação sem EmpresaId válido. Item: {Nome}", ControllerLogPrefix, request.Nome);
            return Unauthorized("Token inválido.");
        }

        request.EmpresaId = empresaId.Value;
        var result = await _mediator.Send(request);

        return HandleResult<object>(result);
    }

    [HttpPatch("update/{id}")]
    public async Task<IActionResult> UpdateEstoque([FromRoute] int id, [FromBody] UpdateItemEstoqueCommand request)
    {
        _logger.LogInformation("{LogPrefix} Atualizando item de estoque. ID: {ItemId} | Novo Nome: {Nome}", ControllerLogPrefix, id, request.Nome);

        var empresaId = User.GetEmpresaId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Falha de autorização na atualização do item {Id}.", ControllerLogPrefix, id);
            return Unauthorized("Token inválido.");
        }

        request.EmpresaId = empresaId.Value;
        request.ItemId = id;

        var result = await _mediator.Send(request);
        return HandleResult<object>(result);
    }

    [HttpPut("add-quantidade/{itemId}")]
    public async Task<IActionResult> AddQuantidadeEstoque([FromRoute] int itemId, [FromBody] AddQuantidadeEstoqueCommand request)
    {
        _logger.LogInformation("{LogPrefix} Adicionando estoque. ID: {ItemId} | Qtd: {Qtd} | Valor: {Valor}", ControllerLogPrefix, itemId, request.QuantidadeAdicionar, request.Valor);

        var empresaId = User.GetEmpresaId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Falha auth na adição de estoque. ID: {ItemId}", ControllerLogPrefix, itemId);
            return Unauthorized("Token inválido.");
        }

        request.EmpresaId = empresaId.Value;
        request.ItemId = itemId;

        var result = await _mediator.Send(request);
        return HandleResult<object>(result);
    }

    [HttpPut("remove-quantidade/{itemId}")]
    public async Task<IActionResult> RemoveQuantidadeEstoque([FromRoute] int itemId, [FromBody] RemoveQuantidadeEstoqueCommand request)
    {
        _logger.LogInformation("{LogPrefix} Baixa de estoque. ID: {ItemId} | Qtd: {Qtd}", ControllerLogPrefix, itemId, request.QuantidadeARemover);

        var empresaId = User.GetEmpresaId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Falha auth na baixa de estoque. ID: {ItemId}", ControllerLogPrefix, itemId);
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
        _logger.LogInformation("{LogPrefix} Excluindo item de estoque permanentemente. ID: {ItemId}", ControllerLogPrefix, id);

        var empresaId = User.GetEmpresaId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Falha auth na exclusão. ID: {ItemId}", ControllerLogPrefix, id);
            return Unauthorized("Token inválido.");
        }

        var result = await _mediator.Send(new DeleteItemEstoqueCommand { EmpresaId = empresaId.Value, ItemId = id });
        return HandleResult<object>(result);
    }
}