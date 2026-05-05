using CakeGestao.Domain.Exceptions;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Empresas.Command.Update;

public class UpdateEmpresaHandler : IRequestHandler<UpdateEmpresaCommand, Result>
{
    private readonly IEmpresaRepository _empresaRepository;
    private readonly IValidator<UpdateEmpresaCommand> _validator;
    private readonly ILogger<UpdateEmpresaHandler> _logger;
    private const string LogPrefix = "[Update Empresa Handler]";

    public UpdateEmpresaHandler(IEmpresaRepository empresaRepository, IValidator<UpdateEmpresaCommand> validator, ILogger<UpdateEmpresaHandler> logger)
    {
        _empresaRepository = empresaRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateEmpresaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Iniciando atualização cadastral. ID: {Id}", LogPrefix, request.Id);

        var validationResult = _validator.Validate(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("{LogPrefix} Dados inválidos para atualização. ID: {Id}. Erros: {Errors}", LogPrefix, request.Id, string.Join(", ", errors));
            return Result.Fail(new ValidationError(errors));
        }

        var empresaResult = await _empresaRepository.GetEmpresaByIdAsync(request.Id);
        if (empresaResult.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Empresa não encontrada para atualização. ID: {Id}", LogPrefix, request.Id);
            return Result.Fail(new NotFoundError("Empresa não encontrada no banco de dados"));
        }
        var empresa = empresaResult.Value;

        empresa.AtualizarDadosCadastrais(request.Nome, request.Endereco);

        await _empresaRepository.UpdateEmpresaAsync(empresa);

        _logger.LogInformation("{LogPrefix} Empresa atualizada com sucesso. ID: {Id}", LogPrefix, request.Id);
        return Result.Ok();
    }
}