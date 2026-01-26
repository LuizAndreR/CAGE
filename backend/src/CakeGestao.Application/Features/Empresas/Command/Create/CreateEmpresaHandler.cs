using AutoMapper;
using CakeGestao.Application.Features.Empresas.Command.Create;
using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.Empresas.UseCase;

public class CreateEmpresaHandler : IRequestHandler<CreateEmpresaCommand, Result>
{
    private readonly IEmpresaRepository _empresaRepository;
    private readonly ILogger<CreateEmpresaHandler> _logger;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateEmpresaCommand> _validator;
    private const string UseCaseLogPrefix = "[Create Empresa]";

    public CreateEmpresaHandler(IEmpresaRepository empresaRepository, ILogger<CreateEmpresaHandler> logger, IMapper mapper, IValidator<CreateEmpresaCommand> validator)
    {
        _empresaRepository = empresaRepository;
        _logger = logger;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<Result> Handle(CreateEmpresaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{UseCaseLogPrefix} Iniciando processo para a empresa de nome: {Nome}", UseCaseLogPrefix, request.Nome);

        _logger.LogInformation("{UseCaseLogPrefix} Validando dados para a empresa de nome: {Nome}", UseCaseLogPrefix, request.Nome);
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("{UseCaseLogPrefix} Validação falhou para a empresa de nome: {Nome}. Erros: {Errors}", UseCaseLogPrefix, request.Nome, errors);
            return Result.Fail(new ValidationError(errors));
        }
        _logger.LogInformation("{UseCaseLogPrefix} Validação para a empresa de nome: {Nome} realizada com sucesso", UseCaseLogPrefix, request.Nome);

        _logger.LogInformation("{UseCaseLogPrefix} Verificando se já existe uma empresa com o nome: {Nome}", UseCaseLogPrefix, request.Nome);
        var existingEmpresaResult = await _empresaRepository.EmpresaExistsByNomeAsync(request.Nome);
        if (existingEmpresaResult.IsFailed)
        {
            _logger.LogWarning("{UseCaseLogPrefix} Já existe uma empresa cadastrada com o nome: {Nome}", UseCaseLogPrefix, request.Nome);
            return Result.Fail(existingEmpresaResult.Errors);
        }
        _logger.LogInformation("{UseCaseLogPrefix} Nenhuma empresa encontrada com o nome: {Nome}. Prosseguindo com o cadastro.", UseCaseLogPrefix, request.Nome);

        _logger.LogInformation("{UseCaseLogPrefix} Iniciando mapeamento da requisição para a entidade Empresa", UseCaseLogPrefix);
        StatusEmpresaEnum status = StatusEmpresaEnum.Ativa;
        Empresa empresaEntity = new Empresa(request.Nome, request.Endereco, status);
        _logger.LogInformation("{UseCaseLogPrefix} Mapeamento concluído com sucesso", UseCaseLogPrefix);

        _logger.LogInformation("{UseCaseLogPrefix} Iniciando persistência da nova empresa no banco de dados", UseCaseLogPrefix);
        await _empresaRepository.CreateEmpresaAsync(empresaEntity);
        _logger.LogInformation("{UseCaseLogPrefix} Persistência da nova empresa de id: {Id} concluída com sucesso", UseCaseLogPrefix, empresaEntity.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Processo para a empresa de nome: {Nome} finalizado com sucesso", UseCaseLogPrefix, request.Nome);
        return Result.Ok();
    }
}
