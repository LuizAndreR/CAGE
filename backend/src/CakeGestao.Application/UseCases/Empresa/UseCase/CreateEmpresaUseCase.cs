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

public class CreateEmpresaUseCase : ICreateEmpresaUseCase
{
    private readonly IEmpresaRepository _empresaRepository;
    private readonly ILogger<CreateEmpresaUseCase> _logger;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateEmpresaRequest> _validator;

    public CreateEmpresaUseCase(IEmpresaRepository empresaRepository, ILogger<CreateEmpresaUseCase> logger, IMapper mapper, IValidator<CreateEmpresaRequest> validator)
    {
        _empresaRepository = empresaRepository;
        _logger = logger;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<Result> ExecuteAsync(CreateEmpresaRequest request)
    {
        _logger.LogInformation("Iniciando o processo de cadastro de uma noma emprasa de nome: {Nome}", request.Nome);

        _logger.LogInformation("Validando os dados da nova empresa");
        var validationResult = _validator.Validate(request);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validação falhou para o cadastro da nova empresa de nome: {Nome}", request.Nome);
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result.Fail(new ValidationError(errors));
        }
        _logger.LogInformation("Dados da nova empresa validados com sucesso");

        _logger.LogInformation("Verificando se já existe uma empresa cadastrada com o nome: {Nome}", request.Nome);
        var existingEmpresa = await _empresaRepository.EmpresaExistsByNomeAsync(request.Nome);
        if (existingEmpresa.IsFailed)
        {
            _logger.LogWarning("Já existe uma empresa cadastrada com o nome: {Nome}", request.Nome);
            return Result.Fail(new ConflictError("Já existe uma empresa cadastrada com o mesmo nome"));
        }
        _logger.LogInformation("Nenhuma empresa existente com o nome: {Nome} foi encontrada", request.Nome);

        _logger.LogInformation("Mapeando os dados da nova empresa para a entidade Empresa");
        var empresaEntity = _mapper.Map<Empresa>(request);
        empresaEntity.DataCadastro = DateTime.UtcNow;
        empresaEntity.Status = StatusEmpresaEnum.Pendente;
        _logger.LogInformation("Mapeamento realizado com sucesso");

        _logger.LogInformation("Cadastrando a nova empresa no repositório");
        await _empresaRepository.CreateEmpresaAsync(empresaEntity);
        _logger.LogInformation("Empresa cadastrada com sucesso com o ID: {Id}", empresaEntity.Id);
        return Result.Ok();
    }
}
