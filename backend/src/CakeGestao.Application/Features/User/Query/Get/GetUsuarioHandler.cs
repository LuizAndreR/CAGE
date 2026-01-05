using AutoMapper;
using CakeGestao.Application.Features.User.Common;
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
    private const string UseCaseLogPrefix = "[Get Usuario]";

    public GetUsuarioHandler(ILogger<GetUsuarioHandler> logger, IUsuarioRepository usuarioRepository, IMapper mapper)
    {
        _logger = logger;
        _usuarioRepository = usuarioRepository;
        _mapper = mapper;
    }

    public async Task<Result<UsuarioResponse>> Handle(GetUsuarioQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{UseCaseLogPrefix} Iniciando processo para o usuário de id: {Id}", UseCaseLogPrefix, request.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Buscando usuário no repositório. UsuarioId: {Id}", UseCaseLogPrefix, request.Id);
        var usuarioResult = await _usuarioRepository.GetByIdAsync(request.Id);
        if (usuarioResult.IsFailed)
        {
            _logger.LogWarning("{UseCaseLogPrefix} Usuário não encontrado no repositório. UsuarioId: {Id}. Erros: {@Errors}", UseCaseLogPrefix, request.Id, usuarioResult.Errors);
            return Result.Fail(new NotFoundError("Usuário não encontrado"));
        }
        _logger.LogInformation("{UseCaseLogPrefix} Usuário encontrado com sucesso. UsuarioId: {Id}", UseCaseLogPrefix, request.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Iniciando mapeamento da entidade Usuario para UsuarioResponse. UsuarioId: {Id}", UseCaseLogPrefix, request.Id);
        var usuarioResponse = _mapper.Map<UsuarioResponse>(usuarioResult.Value);
        _logger.LogInformation("{UseCaseLogPrefix} Mapeamento concluído com sucesso. UsuarioId: {Id}", UseCaseLogPrefix, request.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Processo finalizado com sucesso para o usuário de id: {Id}", UseCaseLogPrefix, request.Id);
        return Result.Ok(usuarioResponse);
    }
}
