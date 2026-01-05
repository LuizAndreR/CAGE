using CakeGestao.API.Extensions;
using CakeGestao.Application.Features.User.Command.Delete;
using CakeGestao.Application.Features.User.Command.UpdateFuncionario;
using CakeGestao.Application.Features.User.Command.UpdateSenhaUsuario;
using CakeGestao.Application.Features.User.Command.UpdateUser;
using CakeGestao.Application.Features.User.Query.Get;
using CakeGestao.Application.Features.User.Query.GetAll;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CakeGestao.API.Controllers;

[ApiController]
[Authorize]
[Route("api/user/")]
public class UserController : ApiControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IMediator _mediator;
    
    public UserController(ILogger<UserController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [HttpGet("getall")]
    [Authorize(Roles = "Admin, Dono")]
    public async Task<IActionResult> GetAllUsers()
    {
        _logger.LogInformation("ecebendo requisição para obter todos os usuários cadastro no banco de dados");

        var listUserResult = await _mediator.Send(new GetAllUsuarioQuery());
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
        var userResult = await _mediator.Send(new GetUsuarioQuery{Id = id.Value});

        return HandleResult(userResult);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateUsuario([FromBody]UpdateUsuarioCommand request)
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
        request.Id = id.Value;
        var userResult = await _mediator.Send(request);

        return HandleResult<object>(userResult);
    }

    [HttpPut("updatesenha")]
    public async Task<IActionResult> UpdateSenhaUsuario([FromBody]UpdateSenhaUsuarioCommand request)
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
        request.Id = id.Value;  
        var userResult = await _mediator.Send(request);

        return HandleResult<object>(userResult);
    }

    [HttpPatch("updatefuncionario")]
    [Authorize(Roles = "Admin, Dono")]
    public async Task<IActionResult> UpdateFuncaoUsuario([FromBody] UpdateFuncionarioCommand request)
    {
        _logger.LogInformation("Recebendo requisição para update da função do usuario");
        var userResult = await _mediator.Send(request);
        return HandleResult<object>(userResult);
    }

    [HttpDelete("delete/{id}")]
    [Authorize(Roles = "Admin, Dono")]
    public async Task<IActionResult> DeleteUsuario([FromRoute]int id)
    {
        _logger.LogInformation("Recebendo requisição para deletar um usuário");
        int idValue = id;
        var userResult = await _mediator.Send(new DeleteUsuarioCommand{Id=idValue});
        return HandleResult<object>(userResult);
    }
}
