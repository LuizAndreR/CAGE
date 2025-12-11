using System.Linq;
using AutoMapper;
using CakeGestao.Application.Dtos.Requests.Usuario;
using CakeGestao.Application.UseCases.User.Interface;
using CakeGestao.Domain.Enun;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.User.UseCase;

public class UpdateFuncionarioUseCase : IUpdateFuncionarioUseCase
{
    private readonly ILogger<UpdateFuncionarioUseCase> _logger;   
    private readonly IUsuarioRepository _repositoryUser;
    private readonly IValidator<UpdateFuncionarioUsuarioRequest> _validator;
    private readonly IMapper _mapper;
    private const string UseCaseLogPrefix = "[Update Funcionario]";

    public UpdateFuncionarioUseCase(ILogger<UpdateFuncionarioUseCase> logger, IUsuarioRepository repositoryUser, IValidator<UpdateFuncionarioUsuarioRequest> validator, IMapper mapper)
    {
        _logger = logger;
        _repositoryUser = repositoryUser;
        _validator = validator;
        _mapper = mapper;
    }

    public async Task<Result> ExecuteAsync(UpdateFuncionarioUsuarioRequest request)
    {
        _logger.LogInformation("{UseCaseLogPrefix} Iniciando processo de atualização de funcionário. UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Validando requisição para UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);
        var validationResult = _validator.Validate(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("{UseCaseLogPrefix} Validação falhou para UsuarioId: {UsuarioId}. Erros: {Errors}", UseCaseLogPrefix, request.Id, errors);
            return Result.Fail(new ValidationError(errors));
        }
        _logger.LogInformation("{UseCaseLogPrefix} Validação concluída com sucesso para UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Buscando usuário no repositório. UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);
        var usuarioResult = await _repositoryUser.GetByIdAsync(request.Id);
        if (usuarioResult.IsFailed)
        {
            _logger.LogWarning("{UseCaseLogPrefix} Usuário não encontrado. UsuarioId: {UsuarioId}. Erros: {@Errors}", UseCaseLogPrefix, request.Id, usuarioResult.Errors);
            return Result.Fail(new NotFoundError("Usuario nao foi encontrado no banco de dados"));
        }
        var usuario = usuarioResult.Value;
        _logger.LogInformation("{UseCaseLogPrefix} Usuário encontrado. UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Verificando se há alterações para UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);
        if (usuario.Nome == request.Nome && usuario.Role.ToString().Equals(request.Role, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("{UseCaseLogPrefix} Nenhuma alteração detectada para UsuarioId: {UsuarioId}. Operação cancelada.", UseCaseLogPrefix, request.Id);
            return Result.Ok();
        }
        _logger.LogInformation("{UseCaseLogPrefix} Alterações detectadas para UsuarioId: {UsuarioId}. Prosseguindo.", UseCaseLogPrefix, request.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Iniciando mapeamento das alterações para UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);
        usuario = _mapper.Map(request, usuario);
        _logger.LogInformation("{UseCaseLogPrefix} Mapeamento concluído para UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Iniciando persistência das alterações para UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);
        await _repositoryUser.UpdateUsuarioAsync(usuario);
        _logger.LogInformation("{UseCaseLogPrefix} Persistência concluída com sucesso para UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Processo de atualização finalizado com sucesso para UsuarioId: {UsuarioId}", UseCaseLogPrefix, request.Id);
        return Result.Ok();
    }
}
