using CakeGestao.Application.Features.Financeiro.Command.Create;
using CakeGestao.Domain.Exceptions;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CakeGestao.Application.Features.Estoque.Command.AddQuantidade;

public class AddQuantidadeEstoqueHandler : IRequestHandler<AddQuantidadeEstoqueCommand, Result>
{
    private readonly IEstoqueRepository _estoqueRepository;
    private readonly ILogger<AddQuantidadeEstoqueHandler> _logger;
    private readonly IValidator<AddQuantidadeEstoqueCommand> _validator;
    private readonly IMediator _mediator;
    private const string LogPrefix = "[Add Estoque Handler]";

    public AddQuantidadeEstoqueHandler(IEstoqueRepository estoqueRepository, ILogger<AddQuantidadeEstoqueHandler> logger, IValidator<AddQuantidadeEstoqueCommand> validator, IMediator  mediator)
    {
        _estoqueRepository = estoqueRepository;
        _logger = logger;
        _validator = validator;
        _mediator = mediator;   
    }

    public async Task<Result> Handle(AddQuantidadeEstoqueCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Iniciando adição de estoque. ItemID: {ItemId} | Qtd: {Qtd} | Valor: {Valor}", LogPrefix, request.ItemId, request.QuantidadeAdicionar, request.Valor);

        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("{LogPrefix} Dados inválidos. ItemID: {ItemId}. Erros: {Errors}", LogPrefix, request.ItemId, string.Join(", ", errors));
            return Result.Fail(new ValidationError(errors));
        }
 
        var itemEstoqueResult = await _estoqueRepository.GetItemEstoqueByIdAsync(request.ItemId, request.EmpresaId);
        if (itemEstoqueResult.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Item não encontrado. ItemID: {ItemId}", LogPrefix, request.ItemId);
            return Result.Fail(new NotFoundError("Item não encontrodo no banco de dados"));
        }
        var itemEstoque = itemEstoqueResult.Value;

        itemEstoque.AdicionarQuantidade(request.QuantidadeAdicionar, request.Valor);
        var financeiroResult = await _mediator.Send(new CreateTransacaoCommand
        {
            EmpresaId =  request.EmpresaId,
            Tipo = "Saida",
            Categoria = "Compras",
            Data = DateTime.UtcNow,
            Descricao = $"Compra de {request.QuantidadeAdicionar} {itemEstoque.UnidadeMedida.ToString()} de {itemEstoque.Nome}",
            Valor =  request.Valor
        });
        
        if (financeiroResult.IsFailed)
        {
            var errors = string.Join("; ", financeiroResult.Errors.Select(e => e.Message));
            _logger.LogWarning("{LogPrefix} Falha ao registrar transação financeira. Ação abortada. Erros: {Errors}", LogPrefix, errors);
            return Result.Fail(new ValidationError(new List<string> { "Não foi possível registrar a despesa financeira da compra via estoque." }));
        }
        
        await _estoqueRepository.UpdateItemEstoqueAsync(itemEstoque);
        _logger.LogInformation("{LogPrefix} Estoque atualizado com sucesso. ID: {Id} | Nova Qtd: {Qtd} | Novo Valor Médio: {Media}", LogPrefix, itemEstoque.Id, itemEstoque.QuantidadeAtual, itemEstoque.ValorMedia);

        return Result.Ok();
    }
}
