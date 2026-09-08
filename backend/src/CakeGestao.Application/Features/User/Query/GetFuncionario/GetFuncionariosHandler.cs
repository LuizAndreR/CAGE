using AutoMapper;
using CakeGestao.Application.Features.User.Common;
using CakeGestao.Domain.Exceptions;
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
    private const string LogPrefix = "[Get Funcionarios Handler]";
    public GetFuncionariosHandler(ILogger<GetFuncionariosHandler> logger, IValidator<GetFuncionariosQuery> validator, IUsuarioRepository repository, IMapper mapper)
    {
        _logger = logger;
        _validator = validator;
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<List<UsuarioResponse>>> Handle(GetFuncionariosQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Iniciando listagem de funcionários. EmpresaId: {EmpresaId}", LogPrefix, request.EmpresaId);

        var validatorResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validatorResult.IsValid)
        {
            var errors = validatorResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("{LogPrefix} Requisição inválida. EmpresaId: {EmpresaId}. Erros: {Errors}", LogPrefix, request.EmpresaId, string.Join(", ", errors));
            return Result.Fail(new ValidationError(errors));
        }

        var usuariosResult = await _repository.GetAllUsuariosAsync(request.EmpresaId);
        if (usuariosResult.IsFailed)
        {
            var erro = usuariosResult.Errors.ToString();
            _logger.LogError("{LogPrefix} - Falha ao obter os usuarios: {Errors}", LogPrefix, string.Join(", ", usuariosResult.Errors.Select(e => e.Message)));
            return Result.Fail(new NotFoundError(erro!));
        }

        var usuariosResponse = _mapper.Map<List<UsuarioResponse>>(usuariosResult.Value);

        _logger.LogInformation("{LogPrefix} Listagem concluída. Total de funcionários encontrados: {Count}", LogPrefix, usuariosResponse.Count);

        return Result.Ok(usuariosResponse);
    }
}
