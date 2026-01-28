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
    private const string LogPrefix = "[Get All Usuarios Handler]";

    public GetAllUsuarioHandler(IUsuarioRepository repository, IMapper mapper, ILogger<GetAllUsuarioHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<UsuarioResponse>>> Handle(GetAllUsuarioQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Iniciando listagem de usuários.", LogPrefix);

        var listUsuarioResult = await _repository.GetAllUsuariosAsync(null);
        if (listUsuarioResult.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Falha ao buscar usuários.", LogPrefix);
            return Result.Fail(new NotFoundError("Nenhum usuario comum entrado no banco da dados"));
        }

        var usuarios = listUsuarioResult.Value;
        usuarios.RemoveAll(u => u.Role.ToString() == "Admin");

        if (usuarios.Count == 0)
        {
            _logger.LogWarning("{LogPrefix} Nenhum usuario filtrado por admin encontrado. Total retornado :", LogPrefix);
            return Result.Fail(new NotFoundError("Nenhum usuario comum encontrado no banco de dados"));
        }

        var listUsuario = _mapper.Map<List<UsuarioResponse>>(usuarios);

        _logger.LogInformation("{LogPrefix} Listagem concluída. Total retornado (filtrado): {Count}", LogPrefix, listUsuario.Count);
        return Result.Ok(listUsuario);
    }
}