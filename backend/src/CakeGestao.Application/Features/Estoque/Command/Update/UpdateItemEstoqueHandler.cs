using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Estoque.Command.Update;

public class UpdateItemEstoqueHandler : IRequestHandler<UpdateItemEstoqueCommand, Result>
{
    private readonly IEstoqueRepository _estoqueRepository;
    private readonly IValidator<UpdateItemEstoqueCommand> _validator;
    private readonly ILogger<UpdateItemEstoqueHandler> _logger;
    private const string UseCaseLogPrefix = "[Update Item Estoque]";

    public UpdateItemEstoqueHandler(IEstoqueRepository estoqueRepository, IValidator<UpdateItemEstoqueCommand> validator, ILogger<UpdateItemEstoqueHandler> logger)
    {
        _estoqueRepository = estoqueRepository;
        _validator = validator;
        _logger = logger;
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

        _logger.LogInformation("{LogPrefix} Atualizando dados cadastrais do item de estoque...", UseCaseLogPrefix);
        var unidadeMedidaAlterada = Enum.Parse<UnidadeMedidaEnum>(request.UnidadeMedida);
        itemEstoque.AtualizarDadosCadastrais(request.Nome, request.QuantidadeAtual, unidadeMedidaAlterada);
        _logger.LogInformation("{LogPrefix} Dados cadastrais do item de estoque atualizados. ItemId: {ItemId}", UseCaseLogPrefix, request.ItemId);

        _logger.LogInformation("{LogPrefix} Persistindo alterações no repositório...", UseCaseLogPrefix);
        await _estoqueRepository.UpdateItemEstoqueAsync(itemEstoque);
        _logger.LogInformation("{LogPrefix} Alterações persistidas com sucesso.", UseCaseLogPrefix);

        _logger.LogInformation("{LogPrefix} Execução do caso de uso concluída com sucesso.", UseCaseLogPrefix);
        return Result.Ok();
    }
}
