using CakeGestao.Domain.Enum;
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
    private const string UseCaseLogPrefix = "[Update Empresa]";

    public UpdateEmpresaHandler(IEmpresaRepository empresaRepository, IValidator<UpdateEmpresaCommand> validator, ILogger<UpdateEmpresaHandler> logger)
    {
        _empresaRepository = empresaRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateEmpresaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{UseCaseLogPrefix} Iniciando processo para a empresa de id: {Id}", UseCaseLogPrefix, request.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Validando dados para a empresa de id: {Id}", UseCaseLogPrefix, request.Id);
        var validationResult = _validator.Validate(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("{UseCaseLogPrefix} Validação falhou para a empresa de id: {Id}. Erros: {Errors}", UseCaseLogPrefix, request.Id, errors);
            return Result.Fail(new ValidationError(errors));
        }
        _logger.LogInformation("{UseCaseLogPrefix} Validação para a empresa de id: {Id} realizada com sucesso", UseCaseLogPrefix, request.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Buscando a empresa de id: {Id} no banco de dados", UseCaseLogPrefix, request.Id);
        var empresaResult = await _empresaRepository.GetEmpresaByIdAsync(request.Id);
        if (empresaResult.IsFailed)
        {
            _logger.LogWarning("{UseCaseLogPrefix} Empresa de id: {Id} não encontrada no banco de dados", UseCaseLogPrefix, request.Id);
            return Result.Fail(new NotFoundError("Empresa não encontrada no banco de dados"));
        }
        var empresa = empresaResult.Value;
        _logger.LogInformation("{UseCaseLogPrefix} Empresa de id: {Id} encontrada com sucesso", UseCaseLogPrefix, request.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Atualizando dados cadastrais da empresa de id: {Id}", UseCaseLogPrefix, request.Id);
        var status = Enum.Parse<StatusEmpresaEnum>(request.Status);
        empresa.AtualizarDadosCadastrais(request.Nome, request.Endereco, status);
        _logger.LogInformation("{UseCaseLogPrefix} Dados cadastrais da empresa de id: {Id} atualizados com sucesso", UseCaseLogPrefix, request.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Iniciando persistência da atualização para a empresa de id: {Id}", UseCaseLogPrefix, request.Id);
        await _empresaRepository.UpdateEmpresaAsync(empresa);
        _logger.LogInformation("{UseCaseLogPrefix} Persistência da atualização para a empresa de id: {Id} concluída com sucesso", UseCaseLogPrefix, request.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Processo para a empresa de id: {Id} finalizado com sucesso", UseCaseLogPrefix, request.Id);
        return Result.Ok();
    }
}