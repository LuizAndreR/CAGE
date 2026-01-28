using CakeGestao.API.Extensions;
using CakeGestao.Application.Features.Auth.Cadastro;
using CakeGestao.Application.Features.Auth.Login;
using CakeGestao.Application.Features.Auth.Refresh;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CakeGestao.API.Controllers;

[ApiController]
[Route("api/auth/")]
public class AuthContoller : ApiControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthContoller> _logger;
    private const string ControllerLogPrefix = "[Auth Controller]";

    public AuthContoller(IMediator mediator, ILogger<AuthContoller> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("cadastro")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CadastroDono([FromBody] CadastroCommand request, [FromQuery] int? id)
    {
        _logger.LogInformation("{LogPrefix} Recebendo requisição de cadastro administrativo (Admin/Dono). Email: {Email}", ControllerLogPrefix, request.Email);

        request.EmpresaId = id >= 0 ? id.Value : 0;
        request.AdminRole = true;
        var result = await _mediator.Send(request);

        return HandleResult<object>(result, _logger, ControllerLogPrefix);
    }

    [HttpPost("cadastrofunc")]
    [Authorize(Roles = "Dono")]
    public async Task<IActionResult> Cadastro([FromBody] CadastroCommand request)
    {
        _logger.LogInformation("{LogPrefix} Recebendo requisição de cadastro de funcionário. Email: {Email}", ControllerLogPrefix, request.Email);

        var empresaId = User.GetEmpresaId();
        if (empresaId.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Falha ao obter EmpresaId do token para o email: {Email}. Erro: {Error}", ControllerLogPrefix, request.Email, empresaId.Errors);
            return Unauthorized();
        }

        request.EmpresaId = empresaId.Value;
        var result = await _mediator.Send(request);

        return HandleResult<object>(result, _logger, ControllerLogPrefix);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginCommand request)
    {
        _logger.LogInformation("{LogPrefix} Recebendo tentativa de login. Email: {Email}", ControllerLogPrefix, request.Email);

        var result = await _mediator.Send(request);

        return HandleResult(result, _logger, ControllerLogPrefix);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand request)
    {
        _logger.LogInformation("{LogPrefix} Recebendo requisição de Refresh Token.", ControllerLogPrefix);

        var result = await _mediator.Send(request);

        return HandleResult(result, _logger, ControllerLogPrefix);
    }
}