using AutoMapper;
using CakeGestao.Application.Features.User.Common;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.User.Query.GetAll;

public class GetAllUsuarioHandler : IRequestHandler<GetAllUsuarioQuery, Result<List<UsuarioResponse>>>
{
    private readonly IUsuarioRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllUsuarioHandler> _logger;
    private const string UseCaseLogPrefix = "[Get All Usuarios]";

    public GetAllUsuarioHandler(IUsuarioRepository repository, IMapper mapper, ILogger<GetAllUsuarioHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<UsuarioResponse>>> Handle(GetAllUsuarioQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{UseCaseLogPrefix} Iniciando processo de listagem de usu�rios", UseCaseLogPrefix);

        _logger.LogInformation("{UseCaseLogPrefix} Buscando todos os usu�rios no reposit�rio", UseCaseLogPrefix);
        var listUsuarioResult = await _repository.GetAllUsuariosAsync();
        if (listUsuarioResult.IsFailed)
        {
            _logger.LogWarning("{UseCaseLogPrefix} Falha ao buscar usu�rios no reposit�rio. Erros: {@Errors}", UseCaseLogPrefix, listUsuarioResult.Errors);
            return Result.Fail(new NotFoundError("Nenhum usuario comum entrado no banco da dados"));
        }
        _logger.LogInformation("{UseCaseLogPrefix} Busca conclu�da. Total retornado: {Total}", UseCaseLogPrefix, listUsuarioResult.Value.Count);

        _logger.LogInformation("{UseCaseLogPrefix} Removendo usu�rios com fun��es administrativas (Dono, Admin) da lista", UseCaseLogPrefix);
        var usuarios = listUsuarioResult.Value;
        usuarios.RemoveAll(u => u.Role.ToString() == "Dono");
        usuarios.RemoveAll(u => u.Role.ToString() == "Admin");
        _logger.LogInformation("{UseCaseLogPrefix} Remo��o de administradores conclu�da. Total de usu�rios comuns ap�s filtro: {TotalFiltered}", UseCaseLogPrefix, usuarios.Count);

        if (usuarios.Count == 0)
        {
            _logger.LogWarning("{UseCaseLogPrefix} Nenhum usu�rio comum encontrado ap�s filtro", UseCaseLogPrefix);
            return Result.Fail(new NotFoundError("Nenhum usu�rio comum encontrado no banco de dados"));
        }

        _logger.LogInformation("{UseCaseLogPrefix} Iniciando mapeamento das entidades de usu�rio para DTOs", UseCaseLogPrefix);
        var listUsuario = _mapper.Map<List<UsuarioResponse>>(usuarios);
        _logger.LogInformation("{UseCaseLogPrefix} Mapeamento conclu�do com sucesso. Total mapeado: {TotalMapped}", UseCaseLogPrefix, listUsuario.Count);

        _logger.LogInformation("{UseCaseLogPrefix} Processo de listagem de usu�rios finalizado com sucesso", UseCaseLogPrefix);
        return Result.Ok(listUsuario);
    }
}