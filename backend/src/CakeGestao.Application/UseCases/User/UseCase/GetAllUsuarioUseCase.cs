using AutoMapper;
using CakeGestao.Application.Dtos.Responses;
using CakeGestao.Application.UseCases.User.Interface;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.User.UseCase;

public class GetAllUsuarioUseCase : IGetAllUsuarioUseCase
{
    private readonly IUsuarioRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllUsuarioUseCase> _logger;

    public GetAllUsuarioUseCase(IUsuarioRepository repository, IMapper mapper, ILogger<GetAllUsuarioUseCase> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<UsuarioResponse>>> Execute()
    {
        _logger.LogInformation("Iniciando o processo do GetAllUseCase");
        
        _logger.LogInformation("Iniciando o processo e busca no banco de dados de todos os usuario cadastrados");
        var listUsuarioResult = await _repository.GetAllUsuariosAsync();
        if (listUsuarioResult.IsFailed)
        {
            _logger.LogWarning("Nenhum usuario entrado no banco da dados");
            return Result.Fail(new NotFoundError("Nenhum usuario entrado no banco da dados"));
        }
        _logger.LogInformation("Processo realizado com sucesso na busca de usuario no banco de dados");

        _logger.LogInformation("Removendo usuario com função Dono e Admin da lista de usuarios");
        listUsuarioResult.Value.RemoveAll(u => u.Role.ToString() == "Dono");
        listUsuarioResult.Value.RemoveAll(u => u.Role.ToString() == "Admin");
        if (listUsuarioResult.Value.Count == 0)
        {
            _logger.LogWarning("Nenhum usuario comum entrado no banco da dados");
            return Result.Fail(new NotFoundError("Nenhum usuario comum entrado no banco da dados"));
        }
        _logger.LogInformation("Usuario com função Dono removido com sucesso da lista de usuarios");

        _logger.LogInformation("Iniciando o processo de mapeamento dos usuario entrondo no banco de dados");
        var listUsuario = _mapper.Map<List<UsuarioResponse>>(listUsuarioResult.Value);
        _logger.LogInformation("Mapeamento realizado com susseso");
        
        _logger.LogInformation("Processo de GetAll realizado com susseso");
        return Result.Ok(listUsuario);
    }
}