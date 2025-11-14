using CakeGestao.API.Extensions;
using CakeGestao.Application.Dtos.Requests.Usuario;
using CakeGestao.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CakeGestao.API.Controllers;

[ApiController]
[Authorize]
[Route("api/user/")]
public class UserController : ApiControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserService _userService;
    
    public UserController(ILogger<UserController> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    [HttpGet("getall")]
    [Authorize(Roles = "Admin, Dono")]
    public async Task<IActionResult> GetAllUsers()
    {
        _logger.LogInformation("ecebendo requisição para obter todos os usuários cadastro no banco de dados");

        var listUserResult = await _userService.GetAllUsuarioAsync();
        return HandleResult(listUserResult);
    }

    [HttpGet("getuser")]
    public async Task<IActionResult> GetUserById()
    {
        _logger.LogInformation("Recebendo requisição para obter o usuário");
        
        _logger.LogInformation("Pegando o id do usuairo meio de token");
        var id = User.GetUserId();

        if (id.IsFailed)
        {
            _logger.LogWarning("Token de autorização inválido ou não contém ID.");
            return Unauthorized("Token inválido.");
        }
        
        _logger.LogInformation("Iniciando a requisição para obter o usuário com ID: {Id}", id);
        var userResult = await _userService.GetUsuarioByIdAsync(id.Value);

        return HandleResult(userResult);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateUsuario([FromBody]UpdateUsuarioRequest request)
    {
        _logger.LogInformation("Recebendo requisição para update de usuario");
        
        _logger.LogInformation("Pegando o id do usuairo meio de token");
        var id = User.GetUserId();
        if (id.IsFailed)
        {
            _logger.LogWarning("Token de autorização inválido ou não contém ID.");
            return Unauthorized("Token inválido.");
        }
        
        _logger.LogInformation("Iniciando a requisição para update do usuário com ID: {Id}", id);
        var userResult = await _userService.UpdateUsuarioAsync(request, id.Value);

        return HandleResult<object>(userResult);
    }

    [HttpPatch("updatesenha")]
    public async Task<IActionResult> UpdateSenhaUsuario([FromBody]UpdateSenhaUsuarioRequest request)
    {
        _logger.LogInformation("Recebendo requisição para update da senha do usuario");
        
        _logger.LogInformation("Pegando o id do usuairo meio de token");
        var id = User.GetUserId();
        if (id.IsFailed)
        {
            _logger.LogWarning("Token de autorização inválido ou não contém ID.");
            return Unauthorized("Token inválido.");
        }
        
        _logger.LogInformation("Iniciando a requisição para update da senha do usuário com ID: {Id}", id);
        var userResult = await _userService.UpdateSenhaUsuarioAsync(request, id.Value);

        return HandleResult<object>(userResult);
    }

    [HttpPatch("updatefuncionario")]
    [Authorize(Roles = "Admin, Dono")]
    public async Task<IActionResult> UpdateFuncaoUsuario([FromBody] UpdateFuncionarioUsuarioRequest request)
    {
        _logger.LogInformation("Recebendo requisição para update da função do usuario");
        var userResult = await _userService.UpdateFuncionarioAsync(request);
        return HandleResult<object>(userResult);
    }

    [HttpDelete("delete/{id}")]
    [Authorize(Roles = "Admin, Dono")]
    public async Task<IActionResult> DeleteUsuario([FromRoute]int id)
    {
        _logger.LogInformation("Recebendo requisição para deletar um usuário");
        int idValue = id;
        var userResult = await _userService.DeleteUsuarioAsync(id);
        return HandleResult<object>(userResult);
    }
}
