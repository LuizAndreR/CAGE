using AutoMapper;
using CakeGestao.Application.Dtos.Responses;
using CakeGestao.Application.UseCases.User.Interface;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.User.UseCase;

public class GetUsuarioUseCase : IGetUsuarioUseCase
{
    private readonly ILogger<GetUsuarioUseCase> _logger;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IMapper _mapper;
    private const string UseCaseLogPrefix = "[Get Usuario]";

    public GetUsuarioUseCase(ILogger<GetUsuarioUseCase> logger, IUsuarioRepository usuarioRepository, IMapper mapper)
    {
        _logger = logger;
        _usuarioRepository = usuarioRepository;
        _mapper = mapper;
    }

    public async Task<Result<UsuarioResponse>> Execute(int id)
    {
        _logger.LogInformation("{UseCaseLogPrefix} Iniciando processo para o usuário de id: {Id}", UseCaseLogPrefix, id);

        _logger.LogInformation("{UseCaseLogPrefix} Buscando usuário no repositório. UsuarioId: {Id}", UseCaseLogPrefix, id);
        var usuarioResult = await _usuarioRepository.GetByIdAsync(id);
        if (usuarioResult.IsFailed)
        {
            _logger.LogWarning("{UseCaseLogPrefix} Usuário não encontrado no repositório. UsuarioId: {Id}. Erros: {@Errors}", UseCaseLogPrefix, id, usuarioResult.Errors);
            return Result.Fail(new NotFoundError("Usuário não encontrado"));
        }
        _logger.LogInformation("{UseCaseLogPrefix} Usuário encontrado com sucesso. UsuarioId: {Id}", UseCaseLogPrefix, id);

        _logger.LogInformation("{UseCaseLogPrefix} Iniciando mapeamento da entidade Usuario para UsuarioResponse. UsuarioId: {Id}", UseCaseLogPrefix, id);
        var usuarioResponse = _mapper.Map<UsuarioResponse>(usuarioResult.Value);
        _logger.LogInformation("{UseCaseLogPrefix} Mapeamento concluído com sucesso. UsuarioId: {Id}", UseCaseLogPrefix, id);

        _logger.LogInformation("{UseCaseLogPrefix} Processo finalizado com sucesso para o usuário de id: {Id}", UseCaseLogPrefix, id);
        return Result.Ok(usuarioResponse);
    }
}
