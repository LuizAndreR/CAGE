using AutoMapper;
using CakeGestao.Application.Features.User.Common;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.User.Query.GetFuncionario;

public class GetFuncionariosHandler : IRequestHandler<GetFuncionariosQuery, Result<List<UsuarioResponse>>>
{
    private readonly ILogger<GetFuncionariosHandler> _logger;
    private readonly IValidator<GetFuncionariosQuery> _validator;
    private readonly IUsuarioRepository _repository;
    private readonly IMapper _mapper;
    private const string UseCaseLogPrefix = "[Get Funcionarios]";
    public GetFuncionariosHandler(ILogger<GetFuncionariosHandler> logger, IValidator<GetFuncionariosQuery> validator, IUsuarioRepository repository, IMapper mapper)
    {
        _logger = logger;
        _validator = validator;
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<List<UsuarioResponse>>> Handle(GetFuncionariosQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{UseCaseLogPrefix} - Iniciando o processo de get de todos o usuario viculado da empresa de id: {EmpresaId}", UseCaseLogPrefix, request.EmpresaId);

        var validatorResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validatorResult.IsValid)
        {
            _logger.LogWarning("{UseCaseLogPrefix} - Requisição inválida: {Errors}", UseCaseLogPrefix, validatorResult.Errors);
            var erros = validatorResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result.Fail(new ValidationError(erros));
        }

        var usuariosResult = await _repository.GetAllUsuariosAsync(request.EmpresaId);
        if (usuariosResult.IsFailed)
        {
            _logger.LogError("{UseCaseLogPrefix} - Falha ao obter os usuarios: {Errors}", UseCaseLogPrefix, string.Join(", ", usuariosResult.Errors.Select(e => e.Message)));
            return Result.Fail(new NotFoundError(string.Join(", ", usuariosResult.Errors.Select(e => e.Message))));
        }

        var usuariosResponse = _mapper.Map<List<UsuarioResponse>>(usuariosResult.Value);

        _logger.LogInformation("{UseCaseLogPrefix} - Processo de get de todos os usuarios concluído com sucesso", UseCaseLogPrefix);

        return Result.Ok(usuariosResponse);
    }
}
