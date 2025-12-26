using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Empresas.UpdateStatus;

public class UpdateStatusEmpresaHandler : IRequestHandler<UpdateStatusEmpresaCommand, Result>
{
    private readonly ILogger<UpdateStatusEmpresaHandler> _logger;
    private readonly IEmpresaRepository _empresaRepository;
    private readonly IValidator<UpdateStatusEmpresaCommand> _validator;
    private const string UseCaseLogPrefix = "[Update Status Empresa]";

    public UpdateStatusEmpresaHandler(ILogger<UpdateStatusEmpresaHandler> logger, IEmpresaRepository empresaRepository, IValidator<UpdateStatusEmpresaCommand> validator)
    {
        _logger = logger;
        _empresaRepository = empresaRepository;
        _validator = validator;
    }

    public async Task<Result> Handle(UpdateStatusEmpresaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{UseCaseLogPrefix} Iniciando processo para a empresa de id: {Id}", UseCaseLogPrefix, request.EmpresaId);

        _logger.LogInformation("{UseCaseLogPrefix} Validando dados para a empresa de id: {Id}", UseCaseLogPrefix, request.EmpresaId);
        ValidationResult validationResult = _validator.Validate(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            _logger.LogWarning("{UseCaseLogPrefix} Validação falhou para a empresa de id: {Id}. Erros: {Errors}", UseCaseLogPrefix, request.EmpresaId, errors);
            return Result.Fail(errors);
        }
        _logger.LogInformation("{UseCaseLogPrefix} Validação para a empresa de id: {Id} realizada com sucesso", UseCaseLogPrefix, request.EmpresaId);

        _logger.LogInformation("{UseCaseLogPrefix} Buscando a empresa de id: {Id} no banco de dados", UseCaseLogPrefix, request.EmpresaId);
        var empresaResult = await _empresaRepository.GetEmpresaByIdAsync(request.EmpresaId);
        if (empresaResult.IsFailed)
        {
            _logger.LogWarning("{UseCaseLogPrefix} Empresa de id: {Id} não encontrada no banco de dados", UseCaseLogPrefix, request.EmpresaId);
            return Result.Fail(empresaResult.Errors);
        }
        var empresa = empresaResult.Value;
        _logger.LogInformation("{UseCaseLogPrefix} Empresa de id: {Id} encontrada com sucesso", UseCaseLogPrefix, request.EmpresaId);

        _logger.LogInformation("{UseCaseLogPrefix} Atualizando status da empresa de id: {Id}", UseCaseLogPrefix, request.EmpresaId);
        var status = Enum.Parse<StatusEmpresaEnum>(request.Status);
        empresa.AtulizarStatus(status);
        _logger.LogInformation("{UseCaseLogPrefix} Status da empresa de id: {Id} atualizado com sucesso", UseCaseLogPrefix, request.EmpresaId);

        _logger.LogInformation("{UseCaseLogPrefix} Iniciando persistência da atualização para a empresa de id: {Id}", UseCaseLogPrefix, request.EmpresaId);
        await _empresaRepository.UpdateEmpresaAsync(empresa);
        _logger.LogInformation("{UseCaseLogPrefix} Persistência da atualização para a empresa de id: {Id} concluída com sucesso", UseCaseLogPrefix, request.EmpresaId);

        _logger.LogInformation("{UseCaseLogPrefix} Processo para a empresa de id: {Id} finalizado com sucesso", UseCaseLogPrefix, request.EmpresaId);
        return Result.Ok();
    }
}
