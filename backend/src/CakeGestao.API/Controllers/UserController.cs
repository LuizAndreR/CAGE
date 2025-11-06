using CakeGestao.API.Extensions;
using CakeGestao.Application.Dtos.Requests.Usuario;
using CakeGestao.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CakeGestao.API.Controllers;

[ApiController]
[Authorize]
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

    [HttpGet("getall")]
    [Authorize(Roles = "Admin, Dono")]
    public async Task<IActionResult> GetAllUsers()
    {
        _logger.LogInformation("ecebendo requisição para obter todos os usuários cadastro no banco de dados");

        var listUserResult = await _userService.GetAllUsuarioAsync();
        _logger.LogInformation("{Count} Usuários obtido com sucesso. Código HTTP 200.", listUserResult.Value.Count);
        return Ok(listUserResult.Value);
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

        _logger.LogInformation("Usuário com ID: {Id} obtido com sucesso. Código HTTP 200.", id);
        return Ok(userResult.Value);
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
        await _userService.UpdateUsuarioAsync(request, id.Value);

        _logger.LogInformation("Usuário com ID: {Id} atulizado com sucesso. Código HTTP 201.", id.Value);
        return Created();
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
        await _userService.UpdateSenhaUsuarioAsync(request, id.Value);

        _logger.LogInformation("Senha do usuário com ID: {Id} atulizada com sucesso. Código HTTP 201.", id.Value);
        return Created();
    }

    [HttpPatch("updatefuncionario")]
    [Authorize(Roles = "Admin, Dono")]
    public async Task<IActionResult> UpdateFuncaoUsuario([FromBody] UpdateFuncionarioUsuarioRequest request)
    {
        _logger.LogInformation("Recebendo requisição para update da função do usuario");
        await _userService.UpdateFuncionarioAsync(request);
        _logger.LogInformation("Função do usuário com ID: {Id} atulizada com sucesso. Código HTTP 201.", request.Id);
        return Created();
    }

    [HttpDelete("delete/{id}")]
    [Authorize(Roles = "Admin, Dono")]
    public async Task<IActionResult> DeleteUsuario([FromRoute]int id)
    {
        _logger.LogInformation("Recebendo requisição para deletar um usuário");
        int idValue = id;
        await _userService.DeleteUsuarioAsync(id);
        _logger.LogInformation("Usuário com ID: {Id} deletado com sucesso. Código HTTP 204.", idValue);
        return NoContent();
    }
}
