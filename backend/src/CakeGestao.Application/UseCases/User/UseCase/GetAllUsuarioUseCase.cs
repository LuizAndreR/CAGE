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
    private const string UseCaseLogPrefix = "[Get All Usuarios]";

    public GetAllUsuarioUseCase(IUsuarioRepository repository, IMapper mapper, ILogger<GetAllUsuarioUseCase> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<UsuarioResponse>>> Execute()
    {
        _logger.LogInformation("{UseCaseLogPrefix} Iniciando processo de listagem de usuários", UseCaseLogPrefix);

        _logger.LogInformation("{UseCaseLogPrefix} Buscando todos os usuários no repositório", UseCaseLogPrefix);
        var listUsuarioResult = await _repository.GetAllUsuariosAsync();
        if (listUsuarioResult.IsFailed)
        {
            _logger.LogWarning("{UseCaseLogPrefix} Falha ao buscar usuários no repositório. Erros: {@Errors}", UseCaseLogPrefix, listUsuarioResult.Errors);
            return Result.Fail(new NotFoundError("Nenhum usuario comum entrado no banco da dados"));
        }
        _logger.LogInformation("{UseCaseLogPrefix} Busca concluída. Total retornado: {Total}", UseCaseLogPrefix, listUsuarioResult.Value.Count);

        _logger.LogInformation("{UseCaseLogPrefix} Removendo usuários com funções administrativas (Dono, Admin) da lista", UseCaseLogPrefix);
        var usuarios = listUsuarioResult.Value;
        usuarios.RemoveAll(u => u.Role.ToString() == "Dono");
        usuarios.RemoveAll(u => u.Role.ToString() == "Admin");
        _logger.LogInformation("{UseCaseLogPrefix} Remoção de administradores concluída. Total de usuários comuns após filtro: {TotalFiltered}", UseCaseLogPrefix, usuarios.Count);

        if (usuarios.Count == 0)
        {
            _logger.LogWarning("{UseCaseLogPrefix} Nenhum usuário comum encontrado após filtro", UseCaseLogPrefix);
            return Result.Fail(new NotFoundError("Nenhum usuário comum encontrado no banco de dados"));
        }

        _logger.LogInformation("{UseCaseLogPrefix} Iniciando mapeamento das entidades de usuário para DTOs", UseCaseLogPrefix);
        var listUsuario = _mapper.Map<List<UsuarioResponse>>(usuarios);
        _logger.LogInformation("{UseCaseLogPrefix} Mapeamento concluído com sucesso. Total mapeado: {TotalMapped}", UseCaseLogPrefix, listUsuario.Count);

        _logger.LogInformation("{UseCaseLogPrefix} Processo de listagem de usuários finalizado com sucesso", UseCaseLogPrefix);
        return Result.Ok(listUsuario);
    }
}