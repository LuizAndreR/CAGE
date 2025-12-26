using AutoMapper;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.Estoque.Update;

public class UpdateItemEstoqueHandler : IRequestHandler<UpdateItemEstoqueCommand, Result>
{
    private readonly IEstoqueRepository _estoqueRepository;
    private readonly IValidator<UpdateItemEstoqueCommand> _validator;
    private readonly ILogger<UpdateItemEstoqueHandler> _logger;
    private readonly IMapper _mapper;
    private const string UseCaseLogPrefix = "[Update Item Estoque]";

    public UpdateItemEstoqueHandler(IEstoqueRepository estoqueRepository, IValidator<UpdateItemEstoqueCommand> validator, ILogger<UpdateItemEstoqueHandler> logger, IMapper mapper)
    {
        _estoqueRepository = estoqueRepository;
        _validator = validator;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result> Handle(UpdateItemEstoqueCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Iniciando execução do caso de uso.", UseCaseLogPrefix);

        _logger.LogInformation("{LogPrefix} Validando requisição...", UseCaseLogPrefix);
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            _logger.LogInformation("{LogPrefix} Validação falhou: {Errors}", UseCaseLogPrefix, validationResult.Errors);
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result.Fail(new ValidationError(errors));
        }
        _logger.LogInformation("{LogPrefix} Requisição validada com sucesso.", UseCaseLogPrefix);

        _logger.LogInformation("{LogPrefix} Verificando existência do item de estoque a ser atualizado...", UseCaseLogPrefix);
        var itemEstoqueResult = await _estoqueRepository.GetItemEstoqueByIdAsync(request.ItemId);
        if (itemEstoqueResult.IsFailed)
        {
            _logger.LogInformation("{LogPrefix} Item de estoque não encontrado para atualização. ItemId: {ItemId}", UseCaseLogPrefix, request.ItemId);
            return Result.Fail(new NotFoundError("Item de estoque não encontrado para atualização."));
        }
        var itemEstoque = itemEstoqueResult.Value;  
        _logger.LogInformation("{LogPrefix} Item de estoque encontrado para atualização. ItemId: {ItemId}", UseCaseLogPrefix, request.ItemId);

        _logger.LogInformation("{LogPrefix} Verificando se há alterações nos valores do item de estoque...", UseCaseLogPrefix);
        if (itemEstoque.QuantidadeAtual == request.QuantidadeAtual && itemEstoque.Nome == request.Nome && itemEstoque.UnidadeMedida.ToString().Equals(request.UnidadeMedida, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("{LogPrefix} Nenhuma alteração detectada nos valores do item de estoque. ItemId: {ItemId}", UseCaseLogPrefix, request.ItemId);
            return Result.Ok();
        }
        _logger.LogInformation("{LogPrefix} Alterações detectadas nos valores do item de estoque. ItemId: {ItemId}", UseCaseLogPrefix, request.ItemId);

        _logger.LogInformation("{LogPrefix} Atualizando item de estoque com novos valores...", UseCaseLogPrefix);
        _mapper.Map(request, itemEstoque);
        _logger.LogInformation("{LogPrefix} Item de estoque atualizado em memória. ItemId: {ItemId}", UseCaseLogPrefix, request.ItemId);

        _logger.LogInformation("{LogPrefix} Persistindo alterações no repositório...", UseCaseLogPrefix);
        await _estoqueRepository.UpdateItemEstoqueAsync(itemEstoque);
        _logger.LogInformation("{LogPrefix} Alterações persistidas com sucesso.", UseCaseLogPrefix);

        _logger.LogInformation("{LogPrefix} Execução do caso de uso concluída com sucesso.", UseCaseLogPrefix);
        return Result.Ok();
    }
}
