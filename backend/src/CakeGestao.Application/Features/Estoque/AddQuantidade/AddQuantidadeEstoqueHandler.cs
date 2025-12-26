using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.Estoque.AddQuantidade;

public class AddQuantidadeEstoqueHandler : IRequestHandler<AddQuantidadeEstoqueCommand, Result>
{
    private readonly IEstoqueRepository _estoqueRepository;
    private readonly ILogger<AddQuantidadeEstoqueCommand> _logger;
    private readonly IValidator<AddQuantidadeEstoqueCommand> _validator;
    private const string UseCaseLogPrefix = "[Add Quantidade Estoque]";

    public AddQuantidadeEstoqueHandler(IEstoqueRepository estoqueRepository, ILogger<AddQuantidadeEstoqueCommand> logger, IValidator<AddQuantidadeEstoqueCommand> validator)
    {
        _estoqueRepository = estoqueRepository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result> Handle(AddQuantidadeEstoqueCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{UseCaseLogPrefix} Iniciando processo de adição de quantidade ao estoque. EstoqueId: {EstoqueId}, QuantidadeAdicionar: {QuantidadeAdicionar}, Valor: {Valor}", UseCaseLogPrefix, request.ItemId, request.QuantidadeAdicionar, request.Valor);

        _logger.LogInformation("{UseCaseLogPrefix} Iniciando validação dos dados da requisição. EstoqueId: {EstoqueId}, QuantidadeAdicionar: {QuantidadeAdicionar}, Valor: {Valor}", UseCaseLogPrefix, request.ItemId, request.QuantidadeAdicionar, request.Valor);
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("{UseCaseLogPrefix} Validação falhou para adição de quantidade ao estoque. EstoqueId: {EstoqueId}. Erros: {Errors}", UseCaseLogPrefix, request.ItemId, errors);
            return Result.Fail(new ValidationError(errors));
        }
        _logger.LogInformation("{UseCaseLogPrefix} Validação concluída com sucesso. EstoqueId: {EstoqueId}", UseCaseLogPrefix, request.ItemId);

        _logger.LogInformation("{UseCaseLogPrefix} Verificando existência do item de estoque. EstoqueId: {EstoqueId}", UseCaseLogPrefix, request.ItemId);    
        var itemEstoqueResult = await _estoqueRepository.GetItemEstoqueByIdAsync(request.ItemId);
        if (itemEstoqueResult.IsFailed)
        {
            _logger.LogWarning("{UseCaseLogPrefix} Item de estoque não encontrado. EstoqueId: {EstoqueId}", UseCaseLogPrefix, request.ItemId);
            return Result.Fail(new NotFoundError("Item não encontrodo no banco de dados"));
        }
        var itemEstoque = itemEstoqueResult.Value;
        _logger.LogInformation("{UseCaseLogPrefix} Item de estoque encontrado com sucesso. EstoqueId: {EstoqueId}, Nome: {Nome}", UseCaseLogPrefix, itemEstoque.Id, itemEstoque.Nome);
        
        _logger.LogInformation("{UseCaseLogPrefix} Atualizando quantidade e valor unitário do item de estoque. EstoqueId: {EstoqueId}, QuantidadeAdicionar: {QuantidadeAdicionar}, Valor: {Valor}", UseCaseLogPrefix, request.ItemId, request.QuantidadeAdicionar, request.Valor);
        itemEstoque.AdicionarQuantidade(request.QuantidadeAdicionar, request.Valor);
        _logger.LogInformation("{UseCaseLogPrefix} Atualizando item de estoque com nova quantidade e valor unitário. EstoqueId: {EstoqueId}, NovaQuantidade: {NovaQuantidade}, NovoValorUnitario: {NovoValorUnitario}", UseCaseLogPrefix, itemEstoque.Id, itemEstoque.QuantidadeAtual, itemEstoque.ValorMedia);

        _logger.LogInformation("{UseCaseLogPrefix} Persistindo alterações no repositório. EstoqueId: {EstoqueId}", UseCaseLogPrefix, itemEstoque.Id);
        await _estoqueRepository.UpdateItemEstoqueAsync(itemEstoque);
        _logger.LogInformation("{UseCaseLogPrefix} Item de estoque atualizado com sucesso. EstoqueId: {EstoqueId}, QuantidadeAtual: {QuantidadeAtual}, ValorMedia: {ValorMedia}", UseCaseLogPrefix, itemEstoque.Id, itemEstoque.QuantidadeAtual, itemEstoque.ValorMedia);

        _logger.LogInformation("{UseCaseLogPrefix} Processo de adição de quantidade ao estoque finalizado com sucesso. EstoqueId: {EstoqueId}", UseCaseLogPrefix, itemEstoque.Id);
        return Result.Ok();
    }
}
