using AutoMapper;
using CakeGestao.Application.Features.Empresas.Command.Create;
using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Exceptions;
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
    private readonly IValidator<CreateEmpresaCommand> _validator;
    private const string LogPrefix = "[Create Empresa Handler]";

    public CreateEmpresaHandler(IEmpresaRepository empresaRepository, ILogger<CreateEmpresaHandler> logger, IValidator<CreateEmpresaCommand> validator)
    {
        _empresaRepository = empresaRepository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result> Handle(CreateEmpresaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Iniciando criação de empresa. Nome: {Nome}", LogPrefix, request.Nome);

        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("{LogPrefix} Dados inválidos para criação de empresa. Nome: {Nome}. Erros: {Errors}", LogPrefix, request.Nome, string.Join(", ", errors));
            return Result.Fail(new ValidationError(errors));
        }

        var existingEmpresaResult = await _empresaRepository.EmpresaExistsByNomeAsync(request.Nome);
        if (existingEmpresaResult.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Tentativa de cadastro duplicado. Empresa já existe: {Nome}", LogPrefix, request.Nome);
            return Result.Fail(new ConflictError("Empresa com mesmo nome ja cadastrada"));
        }

        StatusEmpresaEnum status = StatusEmpresaEnum.Ativa;
        Empresa empresaEntity = new Empresa(request.Nome, request.Endereco, status);

        await _empresaRepository.CreateEmpresaAsync(empresaEntity);

        _logger.LogInformation("{LogPrefix} Empresa criada com sucesso. ID: {Id} | Nome: {Nome}", LogPrefix, empresaEntity.Id, empresaEntity.Nome);
        return Result.Ok();
    }
}
