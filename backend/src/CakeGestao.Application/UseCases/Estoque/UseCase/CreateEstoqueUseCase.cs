using AutoMapper;
using CakeGestao.Application.Dtos.Requests.Estoque;
using CakeGestao.Application.UseCases.Estoque.Interface;
using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.Estoque.UseCase;

public class CreateEstoqueUseCase : ICreateEstoqueUseCase
{
    private readonly IEstoqueRepository _estoqueRepository;
    private readonly ILogger<CreateEstoqueUseCase> _logger;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateEstoqueRequest> _validator;
    private const string UseCaseLogPrefix = "[Create Estoque]";

    public CreateEstoqueUseCase(IEstoqueRepository estoqueRepository, ILogger<CreateEstoqueUseCase> logger, IMapper mapper, IValidator<CreateEstoqueRequest> validator)
    {
        _estoqueRepository = estoqueRepository;
        _logger = logger;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<Result> ExecuteAsync(CreateEstoqueRequest request)
    {
        _logger.LogInformation("{UseCaseLogPrefix} Iniciando processo de criação de item de estoque. EmpresaId: {EmpresaId}, Nome: {Nome}", UseCaseLogPrefix, request.EmpresaId, request.Nome);

        _logger.LogInformation("{UseCaseLogPrefix} Iniciando validação dos dados da requisição. EmpresaId: {EmpresaId}, Nome: {Nome}", UseCaseLogPrefix, request.EmpresaId, request.Nome);
        ValidationResult validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("{UseCaseLogPrefix} Validação falhou para novo item de estoque. EmpresaId: {EmpresaId}, Nome: {Nome}. Erros: {Errors}", UseCaseLogPrefix, request.EmpresaId, request.Nome, errors);
            return Result.Fail(new ValidationError(errors));
        }
        _logger.LogInformation("{UseCaseLogPrefix} Validação concluída com sucesso. EmpresaId: {EmpresaId}, Nome: {Nome}", UseCaseLogPrefix, request.EmpresaId, request.Nome);

        _logger.LogInformation("{UseCaseLogPrefix} Verificando existência de item de estoque com o mesmo nome. EmpresaId: {EmpresaId}, Nome: {Nome}", UseCaseLogPrefix, request.EmpresaId, request.Nome);
        var existingItemResult = await _estoqueRepository.ExistItemByNome(request.Nome, request.EmpresaId);
        if(existingItemResult.IsSuccess)
        {
            _logger.LogWarning("{UseCaseLogPrefix} Já existe um item de estoque com o nome fornecido. EmpresaId: {EmpresaId}, Nome: {Nome}", UseCaseLogPrefix, request.EmpresaId, request.Nome);
            return Result.Fail(new ConflictError("Já existe um item de estoque com o nome fornecido."));
        }
        _logger.LogInformation("{UseCaseLogPrefix} Nenhum item de estoque existente com o mesmo nome encontrado. EmpresaId: {EmpresaId}, Nome: {Nome}", UseCaseLogPrefix, request.EmpresaId, request.Nome);

        _logger.LogInformation("{UseCaseLogPrefix} Iniciando mapeamento da requisição para entidade ItemEstoque. EmpresaId: {EmpresaId}, Nome: {Nome}, Quantidade: {Quantidade}", UseCaseLogPrefix, request.EmpresaId, request.Nome, request.QuantidadeAtual);
        var itemEstoque = _mapper.Map<ItemEstoque>(request);
        _logger.LogInformation("{UseCaseLogPrefix} Mapeamento concluído em memória. EmpresaId: {EmpresaId}, Nome: {Nome}", UseCaseLogPrefix, itemEstoque.EmpresaId, itemEstoque.Nome);

        _logger.LogInformation("{UseCaseLogPrefix} Iniciando persistência do item de estoque no repositório. EmpresaId: {EmpresaId}, Nome: {Nome}", UseCaseLogPrefix, itemEstoque.EmpresaId, itemEstoque.Nome);
        await _estoqueRepository.CreateItemEstoqueAsync(itemEstoque);
        _logger.LogInformation("{UseCaseLogPrefix} Persistência concluída com sucesso. EmpresaId: {EmpresaId}, Nome: {Nome}, ItemEstoqueId: {ItemEstoqueId}", UseCaseLogPrefix, itemEstoque.EmpresaId, itemEstoque.Nome, itemEstoque.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Processo de criação de item de estoque finalizado com sucesso. EmpresaId: {EmpresaId}, Nome: {Nome}, ItemEstoqueId: {ItemEstoqueId}", UseCaseLogPrefix, itemEstoque.EmpresaId, itemEstoque.Nome, itemEstoque.Id);
        return Result.Ok();
    }
}
