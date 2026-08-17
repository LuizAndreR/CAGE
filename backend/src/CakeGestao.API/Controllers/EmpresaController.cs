using CakeGestao.API.Extensions;
using CakeGestao.Application.Features.Empresas.Command.Create; 
using CakeGestao.Application.Features.Empresas.Command.Update;
using CakeGestao.Application.Features.Empresas.Command.UpdateStatus;
using CakeGestao.Application.Features.Empresas.Delete;
using CakeGestao.Application.Features.Empresas.Query.Get;
using CakeGestao.Application.Features.Empresas.Query.GetAll;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CakeGestao.API.Controllers;

[ApiController]
[Route("api/empresa")]
[Authorize(Roles = "Admin")]
public class EmpresaController : ApiControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<EmpresaController> _logger;
    private const string ControllerLogPrefix = "[Empresa Controller]";

    public EmpresaController(IMediator mediator, ILogger<EmpresaController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("")]
    public async Task<IActionResult> GetAllEmpresas()
    {
        _logger.LogInformation("{LogPrefix} Listando todas as empresas cadastradas.", ControllerLogPrefix);

        var result = await _mediator.Send(new GetAllEmpresaQuery());

        return HandleResult(result, _logger, ControllerLogPrefix);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEmpresaById(int id)
    {
        _logger.LogInformation("{LogPrefix} Buscando detalhes da empresa. ID: {Id}", ControllerLogPrefix, id);

        var result = await _mediator.Send(new GetEmpresaQuery { Id = id });

        return HandleResult(result, _logger, ControllerLogPrefix);
    }

    [HttpPost("")]
    public async Task<IActionResult> CreateEmpresa([FromBody] CreateEmpresaCommand request)
    {
        _logger.LogInformation("{LogPrefix} Iniciando criação de nova empresa. Nome: {Nome}", ControllerLogPrefix, request.Nome);

        var result = await _mediator.Send(request);

        return HandleResult<object>(result, _logger, ControllerLogPrefix);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEmpresa([FromBody] UpdateEmpresaCommand request, [FromRoute] int id)
    {
        _logger.LogInformation("{LogPrefix} Atualizando dados cadastrais da empresa. ID: {Id}", ControllerLogPrefix, id);

        request.Id = id;
        var result = await _mediator.Send(request);

        return HandleResult<object>(result, _logger, ControllerLogPrefix);
    }

    [HttpPut("me")]
    [Authorize(Roles = "Dono")]
    public async Task<IActionResult> UpdateEmpresaDono([FromBody] UpdateEmpresaCommand request)
    {
        var idResult = User.GetEmpresaId();
        if (idResult.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Tentativa de alteração de cargo/função sem ID de empresa válido.", ControllerLogPrefix);
            return Unauthorized("Token inválido.");
        }
        _logger.LogInformation("{LogPrefix} Atualização de empresa solicitada pelo Dono. ID: {Id}", ControllerLogPrefix, idResult.Value);

        request.Id = idResult.Value;
        var result = await _mediator.Send(request);

        return HandleResult<object>(result, _logger, ControllerLogPrefix);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatusEmpresa([FromBody] UpdateStatusEmpresaCommand request, [FromRoute] int id)
    {
        _logger.LogInformation("{LogPrefix} Alterando status da empresa. ID: {Id}", ControllerLogPrefix, id);

        request.EmpresaId = id;
        var result = await _mediator.Send(request);

        return HandleResult<object>(result, _logger, ControllerLogPrefix);
    }

    [HttpDelete("{id}")] 
    public async Task<IActionResult> DeleteEmpresa([FromRoute] int id)
    {
        _logger.LogInformation("{LogPrefix} Iniciando exclusão de empresa. ID: {Id}", ControllerLogPrefix, id);

        var result = await _mediator.Send(new DeleteEmpresaCommand { Id = id });

        return HandleResult<object>(result, _logger, ControllerLogPrefix);
    }
}