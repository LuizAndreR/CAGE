using CakeGestao.Application.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace CakeGestao.API.Controllers;

[ApiController]
[Route("api/user/")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserService _userService;

    public UserController(ILogger<UserController> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    [HttpGet("getuser")]
    public async Task<IActionResult> GetUserById()
    {
        _logger.LogInformation("Recebendo requisição para obter o usuário com ID: {Id}", id);
        var userResult = await _userService.GetUsuarioByIdAsync(id);

        _logger.LogInformation("Usuário com ID: {Id} obtido com sucesso. Código HTTP 200.", id);
        return Ok(userResult.Value);
    }
}
