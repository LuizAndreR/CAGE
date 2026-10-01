using CakeGestao.API.Extensions;
using CakeGestao.Application.Features.User.Command.Delete;
using CakeGestao.Application.Features.User.Command.UpdateFuncionario;
using CakeGestao.Application.Features.User.Command.UpdateSenhaUsuario;
using CakeGestao.Application.Features.User.Command.UpdateUser;
using CakeGestao.Application.Features.User.Query.Get;
using CakeGestao.Application.Features.User.Query.GetAll;
using CakeGestao.Application.Features.User.Query.GetFuncionario;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CakeGestao.API.Controllers;

[ApiController]
[Authorize]
[Route("api/user")]
public class UserController : ApiControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IMediator _mediator;
    private const string ControllerLogPrefix = "[User Controller]";

    public UserController(ILogger<UserController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [HttpGet("")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        _logger.LogInformation("{LogPrefix} Listando usuários cadastrados.", ControllerLogPrefix);

        // Nota: O filtro por EmpresaId deve ser implementado na query handler
        var userResult = await _mediator.Send(new GetAllUsuarioQuery {  });

        return HandleResult(userResult, _logger, ControllerLogPrefix);
    }

    [HttpGet("funcionarios")]
    [Authorize(Roles = "Dono")]
    public async Task<IActionResult> GetFuncionarios()
    {
        var empresaid = User.GetEmpresaId();
        if (empresaid.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Token sem ID de empresa válido.", ControllerLogPrefix);
            return Unauthorized("Token inválido.");
        }
        
        _logger.LogInformation("{LogPrefix} Listando funcionários cadastrados e viculado da empresa de id: {Id}", ControllerLogPrefix, empresaid);

        var userResult = await _mediator.Send(new GetFuncionariosQuery { EmpresaId = empresaid.Value});
        return HandleResult(userResult, _logger, ControllerLogPrefix);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetUserById()
    {
        var id = User.GetUserId();
        if (id.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Token sem ID de usuário válido.", ControllerLogPrefix);
            return Unauthorized("Token inválido.");
        }
        var empresaId = User.GetEmpresaId();

        _logger.LogInformation("{LogPrefix} Buscando dados do próprio perfil. ID: {Id}", ControllerLogPrefix, id.Value);

        var userResult = await _mediator.Send(new GetUsuarioQuery { Id = id.Value , EmpresaId = empresaId.Value});

        return HandleResult(userResult, _logger, ControllerLogPrefix);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateUsuario([FromBody] UpdateUsuarioCommand request)
    {
        var id = User.GetUserId();
        if (id.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Tentativa de update de perfil sem ID válido.", ControllerLogPrefix);
            return Unauthorized("Token inválido.");
        }
        var empresaId = User.GetEmpresaId();

        _logger.LogInformation("{LogPrefix} Atualizando dados do perfil. ID: {Id}", ControllerLogPrefix, id.Value);

        request.Id = id.Value;
        request.EmpresaId = empresaId.Value;
        var userResult = await _mediator.Send(request);

        return HandleResult<object>(userResult, _logger, ControllerLogPrefix);
    }

    [HttpPatch("me/senha")]
    public async Task<IActionResult> UpdateSenhaUsuario([FromBody] UpdateSenhaUsuarioCommand request)
    {
        var id = User.GetUserId();
        if (id.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Tentativa de alteração de senha sem ID válido.", ControllerLogPrefix);
            return Unauthorized("Token inválido.");
        }
        var empresaId = User.GetEmpresaId();
        _logger.LogInformation("{LogPrefix} Iniciando alteração de senha do usuário. ID: {Id}", ControllerLogPrefix, id.Value);

        request.Id = id.Value;
        request.EmpresaId = empresaId.Value;
        var userResult = await _mediator.Send(request);

        return HandleResult<object>(userResult, _logger, ControllerLogPrefix);
    }

    [HttpPatch("{id}/funcao")]
    [Authorize(Roles = "Admin, Dono")]
    public async Task<IActionResult> UpdateFuncaoUsuario([FromBody] UpdateFuncionarioCommand request, [FromRoute] int id, [FromQuery] int empresaId)
    {
        var empresaError = ResolveEmpresa(empresaId, out var empresaAutorizada);
        if (empresaError != null) return empresaError;
        empresaId = empresaAutorizada;
        request.AdminRole = User.IsInRole("Admin");

        _logger.LogInformation("{LogPrefix} Alteração de cargo/função de funcionário solicitada. Solitado pelo usuario de id: {Id}", ControllerLogPrefix, id);
        request.Id = id;
        request.EmpresaId = empresaId;

        var userResult = await _mediator.Send(request);

        return HandleResult<object>(userResult, _logger, ControllerLogPrefix);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin, Dono")]
    public async Task<IActionResult> DeleteUsuario([FromRoute] int id, [FromQuery]int empresaid)
    {
        _logger.LogInformation("{LogPrefix} Solicitação de exclusão de usuário. ID Alvo: {TargetId}", ControllerLogPrefix, id);
        var empresaError = ResolveEmpresa(empresaid, out var empresaAutorizada);
        if (empresaError != null) return empresaError;
        empresaid = empresaAutorizada;

        var userResult = await _mediator.Send(new DeleteUsuarioCommand { Id = id, EmpresaId = empresaid, AdminRole = User.IsInRole("Admin") });

        return HandleResult<object>(userResult, _logger, ControllerLogPrefix);
    }
    private IActionResult? ResolveEmpresa(int requestedEmpresaId, out int empresaId)
    {
        empresaId = 0;
        if (User.IsInRole("Admin"))
        {
            if (requestedEmpresaId <= 0) return BadRequest("Informe um ID de empresa válido.");
            empresaId = requestedEmpresaId;
            return null;
        }

        var tokenEmpresa = User.GetEmpresaId();
        if (tokenEmpresa.IsFailed || tokenEmpresa.Value <= 0) return Unauthorized("Token inválido.");
        if (requestedEmpresaId > 0 && requestedEmpresaId != tokenEmpresa.Value) return Forbid();
        empresaId = tokenEmpresa.Value;
        return null;
    }
}