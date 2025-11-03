using CakeGestao.Application.Dtos.Requests.Auth;
using CakeGestao.Application.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace CakeGestao.API.Controllers;

[ApiController]
[Route("api/auth/")]
public class AuthContoller : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthContoller> _logger;

    public AuthContoller(IAuthService authService, ILogger<AuthContoller> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("cadastro")]
    public async Task<IActionResult> Cadastro([FromBody] CadastroRequest request)
    {
        _logger.LogInformation("Recebendo requisição para cadastro de novo usuário com email: {Email}", (string)request.Email);
        var result = await _authService.CreateUserAsync(request);

        _logger.LogInformation("Usuário com email: {Email} cadastrado com sucesso. Código HTTP 201.", (string)request.Email);
        return Created();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Recebendo requisição de login para o usuário com email: {Email}", (string)request.Email);
        var tokenResponse = await _authService.Login(request);

        _logger.LogInformation("Login realizado com sucesso para o usuário com email: {Email}. Código HTTP 200.", (string)request.Email);
        return Ok(tokenResponse.Value);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        _logger.LogInformation("Recebendo requisição para refresh token.");
        var tokenResponse = await _authService.RefreshToken(request);

        _logger.LogInformation("Refresh token gerado com sucesso. Código HTTP 200.");
        return Ok(tokenResponse.Value);
    }
}
