using AutoMapper;
using CakeGestao.Application.Features.User.Common;
using CakeGestao.Domain.Exceptions;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.User.Query.Get;

public class GetUsuarioHandler : IRequestHandler<GetUsuarioQuery, Result<UsuarioResponse>>
{
    private readonly ILogger<GetUsuarioHandler> _logger;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IMapper _mapper;
    private const string LogPrefix = "[Get Usuario Handler]";

    public GetUsuarioHandler(ILogger<GetUsuarioHandler> logger, IUsuarioRepository usuarioRepository, IMapper mapper)
    {
        _logger = logger;
        _usuarioRepository = usuarioRepository;
        _mapper = mapper;
    }

    public async Task<Result<UsuarioResponse>> Handle(GetUsuarioQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Buscando detalhes do usuário. ID: {Id}", LogPrefix, request.Id);

        var usuarioResult = await _usuarioRepository.GetByIdAsync(request.Id, request.EmpresaId);
        if (usuarioResult.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Usuário não encontrado. ID: {Id}", LogPrefix, request.Id);
            return Result.Fail(new NotFoundError("Usuário não encontrado"));
        }

        var usuarioResponse = _mapper.Map<UsuarioResponse>(usuarioResult.Value);

        _logger.LogInformation("{LogPrefix} Usuário retornado com sucesso. ID: {Id}", LogPrefix, request.Id);
        return Result.Ok(usuarioResponse);
    }
}
