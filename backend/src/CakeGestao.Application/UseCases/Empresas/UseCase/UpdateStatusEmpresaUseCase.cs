using CakeGestao.Application.Dtos.Requests.Empresa;
using CakeGestao.Application.UseCases.Empresas.Interface;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.Empresas.UseCase;

public class UpdateStatusEmpresaUseCase : IUpdateStatusEmpresaUseCase
{
    private readonly ILogger<UpdateStatusEmpresaUseCase> _logger;
    private readonly IEmpresaRepository _empresaRepository;
    private readonly IValidator<UpdateStatusEmpresaRequest> _validator;
    private const string UseCaseLogPrefix = "[Update Status Empresa]";

    public UpdateStatusEmpresaUseCase(ILogger<UpdateStatusEmpresaUseCase> logger, IEmpresaRepository empresaRepository, IValidator<UpdateStatusEmpresaRequest> validator)
    {
        _logger = logger;
        _empresaRepository = empresaRepository;
        _validator = validator;
    }

    public async Task<Result> ExecuteAsync(UpdateStatusEmpresaRequest request, int id)
    {
        _logger.LogInformation("{UseCaseLogPrefix} Iniciando processo para a empresa de id: {Id}", UseCaseLogPrefix, id);

        _logger.LogInformation("{UseCaseLogPrefix} Validando dados para a empresa de id: {Id}", UseCaseLogPrefix, id);
        ValidationResult validationResult = _validator.Validate(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            _logger.LogWarning("{UseCaseLogPrefix} Validação falhou para a empresa de id: {Id}. Erros: {Errors}", UseCaseLogPrefix, id, errors);
            return Result.Fail(errors);
        }
        _logger.LogInformation("{UseCaseLogPrefix} Validação para a empresa de id: {Id} realizada com sucesso", UseCaseLogPrefix, id);

        _logger.LogInformation("{UseCaseLogPrefix} Buscando a empresa de id: {Id} no banco de dados", UseCaseLogPrefix, id);
        var empresaResult = await _empresaRepository.GetEmpresaByIdAsync(id);
        if (empresaResult.IsFailed)
        {
            _logger.LogWarning("{UseCaseLogPrefix} Empresa de id: {Id} não encontrada no banco de dados", UseCaseLogPrefix, id);
            return Result.Fail(empresaResult.Errors);
        }
        _logger.LogInformation("{UseCaseLogPrefix} Empresa de id: {Id} encontrada com sucesso", UseCaseLogPrefix, id);

        _logger.LogInformation("{UseCaseLogPrefix} Iniciando mapeamento do novo status para a empresa de id: {Id}", UseCaseLogPrefix, id);
        var empresa = empresaResult.Value;
        var status = Enum.Parse<StatusEmpresaEnum>(request.Status);
        empresa.Status = status;
        _logger.LogInformation("{UseCaseLogPrefix} Mapeamento do novo status para a empresa de id: {Id} concluído", UseCaseLogPrefix, id);

        _logger.LogInformation("{UseCaseLogPrefix} Iniciando persistência da atualização para a empresa de id: {Id}", UseCaseLogPrefix, id);
        await _empresaRepository.UpdateEmpresaAsync(empresa);
        _logger.LogInformation("{UseCaseLogPrefix} Persistência da atualização para a empresa de id: {Id} concluída com sucesso", UseCaseLogPrefix, id);

        _logger.LogInformation("{UseCaseLogPrefix} Processo para a empresa de id: {Id} finalizado com sucesso", UseCaseLogPrefix, id);
        return Result.Ok();
    }
}
