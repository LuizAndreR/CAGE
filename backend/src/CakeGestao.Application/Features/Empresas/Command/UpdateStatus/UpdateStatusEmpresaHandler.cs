using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Empresas.Command.UpdateStatus;

public class UpdateStatusEmpresaHandler : IRequestHandler<UpdateStatusEmpresaCommand, Result>
{
    private readonly ILogger<UpdateStatusEmpresaHandler> _logger;
    private readonly IEmpresaRepository _empresaRepository;
    private readonly IValidator<UpdateStatusEmpresaCommand> _validator;
    private const string LogPrefix = "[Update Status Empresa Handler]";

    public UpdateStatusEmpresaHandler(ILogger<UpdateStatusEmpresaHandler> logger, IEmpresaRepository empresaRepository, IValidator<UpdateStatusEmpresaCommand> validator)
    {
        _logger = logger;
        _empresaRepository = empresaRepository;
        _validator = validator;
    }

    public async Task<Result> Handle(UpdateStatusEmpresaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Iniciando alteração de status. ID: {Id} | Novo Status (Request): {Status}", LogPrefix, request.EmpresaId, request.Status);

        ValidationResult validationResult = _validator.Validate(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            _logger.LogWarning("{LogPrefix} Dados inválidos. ID: {Id}. Erros: {Errors}", LogPrefix, request.EmpresaId, string.Join(", ", errors));
            return Result.Fail(errors);
        }

        var empresaResult = await _empresaRepository.GetEmpresaByIdAsync(request.EmpresaId);
        if (empresaResult.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Empresa não encontrada. ID: {Id}", LogPrefix, request.EmpresaId);
            return Result.Fail(empresaResult.Errors);
        }
        var empresa = empresaResult.Value;

        var status = Enum.Parse<StatusEmpresaEnum>(request.Status);
        empresa.AtulizarStatus(status);

        await _empresaRepository.UpdateEmpresaAsync(empresa);

        _logger.LogInformation("{LogPrefix} Status atualizado com sucesso. ID: {Id} | Novo Status: {Status}", LogPrefix, request.EmpresaId, request.Status);
        return Result.Ok();
    }
}
