using CakeGestao.API.Extensions;
using CakeGestao.Application.Dtos.Requests.Auth;
using CakeGestao.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CakeGestao.API.Controllers;

[ApiController]
[Route("api/auth/")]
public class AuthContoller : ApiControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthContoller> _logger;

    public AuthContoller(IAuthService authService, ILogger<AuthContoller> logger) 
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("cadastrodono/{empresaId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CadastroDono([FromBody] CadastroRequest request,[FromRoute] int empresaId)
    {
        _logger.LogInformation("Recebendo requisição para cadastro do usuário dono com email: {Email}", request.Email);
        var result = await _authService.CreateUserAsync(request, empresaId);
        return HandleResult<object>(result);
    }

    [HttpPost("cadastro")]
    [Authorize(Roles = "Admin, Dono")]
    public async Task<IActionResult> Cadastro([FromBody] CadastroRequest request)
    {
        _logger.LogInformation("Recebendo requisição para cadastro de novo usuário com email: {Email}", request.Email);
        var empresaId = User.GetEmpresaId();
        var result = await _authService.CreateUserAsync(request, empresaId.Value);
        return HandleResult<object>(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Recebendo requisição de login para o usuário com email: {Email}", request.Email);
        var result = await _authService.Login(request);
        return HandleResult(result);
    }

    [HttpPost("refresh")]
    [Authorize]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        _logger.LogInformation("Recebendo requisição para refresh token.");
        var result = await _authService.RefreshToken(request);
        return HandleResult(result);
    }
}
