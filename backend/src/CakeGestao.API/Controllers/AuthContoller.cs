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

    public AuthContoller(IMediator mediator, ILogger<AuthContoller> logger) 
    {
        _mediator = mediator;
        _logger = logger;
    }
    
    [HttpPost("cadastro")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CadastroDono([FromBody] CadastroCommand request, [FromQuery] int? empresaId)
    {
        _logger.LogInformation("Recebendo requisição para cadastro usuario de role Dono ou Admin com email: {Email}", request.Email);
        request.EmpresaId = empresaId.HasValue ? empresaId.Value : 0;
        var result = await _mediator.Send(request);
        return HandleResult<object>(result);
    }

    [HttpPost("cadastrofunc")]
    [Authorize(Roles = "Admin, Dono")]
    public async Task<IActionResult> Cadastro([FromBody] CadastroCommand request)
    {
        _logger.LogInformation("Recebendo requisição para cadastro de novo usuário com email: {Email}", request.Email);
        var empresaId = User.GetEmpresaId();
        var result = await _mediator.Send(request);
        return HandleResult<object>(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginCommand request)
    {
        _logger.LogInformation("Recebendo requisição de login para o usuário com email: {Email}", request.Email);
        var result = await _mediator.Send(request);
        return HandleResult(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand request)
    {
        _logger.LogInformation("Recebendo requisição para refresh token.");
        var result = await _mediator.Send(request);
        return HandleResult(result);
    }
}
