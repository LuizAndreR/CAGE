using AutoMapper;
using CakeGestao.Application.Dtos.Requests.Estoque;
using CakeGestao.Application.Dtos.Responses;
using CakeGestao.Application.UseCases.Estoque.Interface;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.Estoque.UseCase;

public class GetAlertaEstoqueUseCase : IGetAlertaEstoqueUseCase
{
    private readonly IEstoqueRepository _estoqueService;
    private readonly ILogger<GetAlertaEstoqueUseCase> _logger;
    private readonly IValidator<ItemEstoqueRequest> _validator;
    private readonly IMapper _mapper;
    private const string UseCaseLogPrefix = "[Get Alerta Estoque]";

    public GetAlertaEstoqueUseCase(IEstoqueRepository estoqueService, ILogger<GetAlertaEstoqueUseCase> logger, IValidator<ItemEstoqueRequest> validator, IMapper mapper)
    {
        _estoqueService = estoqueService;
        _logger = logger;
        _validator = validator;
        _mapper = mapper;
    }

    public async Task<Result<List<ItemEstoqueResponse>>> ExecuteAsync(ItemEstoqueRequest request)
    {
        _logger.LogInformation("{LogPrefix} Iniciando execução do caso de uso.", UseCaseLogPrefix);

        _logger.LogInformation("{LogPrefix} Validando requisição...", UseCaseLogPrefix);
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            _logger.LogInformation("{LogPrefix} Validação falhou: {Errors}", UseCaseLogPrefix, validationResult.Errors);
            throw new ValidationException(validationResult.Errors);
        }
        _logger.LogInformation("{LogPrefix} Requisição validada com sucesso.", UseCaseLogPrefix);

        _logger.LogInformation("{LogPrefix} Obtendo itens de estoque com alerta...", UseCaseLogPrefix);
        var itensEstoqueResult = await _estoqueService.GetAlertaEstoqueByEmpresaIdAsync(request.EmpresaId, 5);
        if (itensEstoqueResult.IsFailed)
        {
            _logger.LogInformation("{LogPrefix} Nenhum item de estoque com alerta encontrado para a empresa de id: {EmpresaId}", UseCaseLogPrefix, request.EmpresaId);
            return Result.Fail<List<ItemEstoqueResponse>>(new NotFoundError("Nenhum item de estoque com alerta encontrado."));
        }
        _logger.LogInformation("{LogPrefix} Itens de estoque com alerta obtidos com sucesso. Total de itens: {TotalItems}", UseCaseLogPrefix, itensEstoqueResult.Value.Count());

        _logger.LogInformation("{LogPrefix} Mapeando entidades para DTOs de resposta...", UseCaseLogPrefix);
        var itensEstoqueResponse = _mapper.Map<List<ItemEstoqueResponse>>(itensEstoqueResult.Value);
        _logger.LogInformation("{LogPrefix} Mapeamento concluído com sucesso.", UseCaseLogPrefix);

        _logger.LogInformation("{LogPrefix} Execução do caso de uso concluída com sucesso.", UseCaseLogPrefix);
        return Result.Ok(itensEstoqueResponse);
    }
}
