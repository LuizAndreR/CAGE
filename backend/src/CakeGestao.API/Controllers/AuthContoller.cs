using CakeGestao.Application.Dtos.Requests.Auth;
using CakeGestao.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Roles = "Admin, Dono")]
    public async Task<IActionResult> Cadastro([FromBody] CadastroRequest request)
    {
        _logger.LogInformation("Recebendo requisição para cadastro de novo usuário com email: {Email}", request.Email);
        var result = await _authService.CreateUserAsync(request);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Usuário com email: {Email} cadastrado com sucesso. Código HTTP 201.", request.Email);
            return Created();
        }

        var erro = result.Errors.First();
        var traceId = HttpContext.TraceIdentifier;

        if (erro is ValidationError validationError)
        {
            _logger.LogWarning("Erro de validação ao cadastrar usuário com email: {Email}. Erros: {Errors}. TraceId: {TraceId}", request.Email, string.Join(", ", validationError.Errors), traceId);
            return BadRequest(new
            {
                title = "Erro de Validação",
                status = 400,
                errors = validationError.Errors,
                traceId = traceId
            });
        }
        else if (erro is ConflictError conflictError)
        {
            _logger.LogWarning("Conflito ao cadastrar usuário com email: {Email}. Mensagem: {Message}. TraceId: {TraceId}", request.Email, conflictError.Message, traceId);
            return Conflict(new
            {
                title = "Conflito",
                status = 409,
                error = conflictError.Message,
                traceId = traceId
            });
        }

        _logger.LogError("Erro inesperado ao cadastrar usuário com email: {Email}. Mensagem: {Message}. TraceId: {TraceId}", request.Email, erro.Message, traceId);
        return StatusCode(500, new
        {
            title = "Erro Interno do Servidor",
            status = 500,
            error = "Ocorreu um erro interno no servidor. Tente novamente mais tarde.",
            traceId = traceId
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Recebendo requisição de login para o usuário com email: {Email}", request.Email);
        var tokenResponse = await _authService.Login(request);

        _logger.LogInformation("Login realizado com sucesso para o usuário com email: {Email}. Código HTTP 200.", request.Email);
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
