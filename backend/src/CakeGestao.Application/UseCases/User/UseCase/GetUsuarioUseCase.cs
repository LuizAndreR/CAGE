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

    public GetUsuarioUseCase(ILogger<GetUsuarioUseCase> logger, IUsuarioRepository usuarioRepository, IMapper mapper)
    {
        _logger = logger;
        _usuarioRepository = usuarioRepository;
        _mapper = mapper;
    }

    public async Task<Result<UsuarioResponse>> Execute(int id)
    {
        _logger.LogInformation("Iniciando execução do caso de uso GetUsuarioUseCase para o usuário com ID: {Id}", id);

        _logger.LogInformation("Recuperando dados do usuário com ID: {Id} do repositório", id);
        var usuarioResult = await _usuarioRepository.GetByIdAsync(id);
        if (usuarioResult.IsFailed)
        {
            _logger.LogWarning("Usuário com ID: {Id} não encontrado no repositório", id);
            return Result.Fail(new NotFoundError($"Usuário com ID: {id} não encontrado."));
        }
        _logger.LogInformation("Dados do usuário com ID: {Id} recuperados com sucesso do repositório", id);

        _logger.LogInformation("Mapeando entidade Usuário para DTO UsuarioResponse para o usuário com ID: {Id}", id);
        var usuarioResponse = _mapper.Map<UsuarioResponse>(usuarioResult.Value);
        _logger.LogInformation("Mapeamento concluído com sucesso para o usuário com ID: {Id}", id);

        _logger.LogInformation("Execução do caso de uso GetUsuarioUseCase concluída com sucesso para o usuário com ID: {Id}", id);
        return Result.Ok(usuarioResponse);
    }
}
