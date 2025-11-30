using AutoMapper;
using CakeGestao.Application.Dtos.Requests.Empresa;
using CakeGestao.Application.UseCases.Empresas.Interface;
using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.Empresas.UseCase;

public class UpdateEmpresaUseCase : IUpdateEmpresaUseCase
{
    private readonly IEmpresaRepository _empresaRepository;
    private readonly IValidator<UpdateEmpresaRequest> _validator;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateEmpresaUseCase> _logger;
    private const string UseCaseLogPrefix = "[Update Empresa]";

    public UpdateEmpresaUseCase(IEmpresaRepository empresaRepository, IValidator<UpdateEmpresaRequest> validator, IMapper mapper, ILogger<UpdateEmpresaUseCase> logger)
    {
        _empresaRepository = empresaRepository;
        _validator = validator;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(UpdateEmpresaRequest request, int id)
    {
        _logger.LogInformation("{UseCaseLogPrefix} Iniciando processo para a empresa de id: {Id}", UseCaseLogPrefix, id);

        _logger.LogInformation("{UseCaseLogPrefix} Validando dados para a empresa de id: {Id}", UseCaseLogPrefix, id);
        var validationResult = _validator.Validate(request);
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

        _logger.LogInformation("{UseCaseLogPrefix} Verificando se houve alterações nos dados da empresa de id: {Id}", UseCaseLogPrefix, id);
        var empresa = empresaResult.Value;
        var status = Enum.Parse<StatusEmpresaEnum>(request.Status);
        if (empresa.Nome == request.Nome && empresa.Endereco == request.Endereco && empresa.Status == status)
        {
            _logger.LogWarning("{UseCaseLogPrefix} Nenhuma alteração foi detectada para a empresa de id: {Id}. A atualização não será realizada.", UseCaseLogPrefix, id);
            return Result.Ok();
        }
        _logger.LogInformation("{UseCaseLogPrefix} Alterações detectadas. Prosseguindo com a atualização para a empresa de id: {Id}", UseCaseLogPrefix, id);

        _logger.LogInformation("{UseCaseLogPrefix} Iniciando mapeamento dos novos dados para a empresa de id: {Id}", UseCaseLogPrefix, id);
        empresa.Nome = request.Nome;
        empresa.Endereco = request.Endereco;
        empresa.Status = status;
        _logger.LogInformation("{UseCaseLogPrefix} Mapeamento dos novos dados para a empresa de id: {Id} concluído", UseCaseLogPrefix, id);

        _logger.LogInformation("{UseCaseLogPrefix} Iniciando persistência da atualização para a empresa de id: {Id}", UseCaseLogPrefix, id);
        await _empresaRepository.UpdateEmpresaAsync(empresa);
        _logger.LogInformation("{UseCaseLogPrefix} Persistência da atualização para a empresa de id: {Id} concluída com sucesso", UseCaseLogPrefix, id);

        _logger.LogInformation("{UseCaseLogPrefix} Processo para a empresa de id: {Id} finalizado com sucesso", UseCaseLogPrefix, id);
        return Result.Ok();
    }
}
