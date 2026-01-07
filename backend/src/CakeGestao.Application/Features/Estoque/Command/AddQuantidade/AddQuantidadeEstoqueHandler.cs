using CakeGestao.Application.Features.Financeiro.Create;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Estoque.Command.AddQuantidade;

public class AddQuantidadeEstoqueHandler : IRequestHandler<AddQuantidadeEstoqueCommand, Result>
{
    private readonly IEstoqueRepository _estoqueRepository;
    private readonly ILogger<AddQuantidadeEstoqueCommand> _logger;
    private readonly IValidator<AddQuantidadeEstoqueCommand> _validator;
    private readonly IMediator _mediator;
    private const string UseCaseLogPrefix = "[Add Quantidade Estoque]";

    public AddQuantidadeEstoqueHandler(IEstoqueRepository estoqueRepository, ILogger<AddQuantidadeEstoqueCommand> logger, IValidator<AddQuantidadeEstoqueCommand> validator, IMediator  mediator)
    {
        _estoqueRepository = estoqueRepository;
        _logger = logger;
        _validator = validator;
        _mediator = mediator;   
    }

    public async Task<Result> Handle(AddQuantidadeEstoqueCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{UseCaseLogPrefix} Iniciando processo de adição de quantidade ao estoque. EstoqueId: {EstoqueId}, QuantidadeAdicionar: {QuantidadeAdicionar}, Valor: {Valor}", UseCaseLogPrefix, request.ItemId, request.QuantidadeAdicionar, request.Valor);
        
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("{UseCaseLogPrefix} Validação falhou para adição de quantidade ao estoque. EstoqueId: {EstoqueId}. Erros: {Errors}", UseCaseLogPrefix, request.ItemId, errors);
            return Result.Fail(new ValidationError(errors));
        }
 
        var itemEstoqueResult = await _estoqueRepository.GetItemEstoqueByIdAsync(request.ItemId);
        if (itemEstoqueResult.IsFailed)
        {
            _logger.LogWarning("{UseCaseLogPrefix} Item de estoque não encontrado. EstoqueId: {EstoqueId}", UseCaseLogPrefix, request.ItemId);
            return Result.Fail(new NotFoundError("Item não encontrodo no banco de dados"));
        }
        var itemEstoque = itemEstoqueResult.Value;
        
        itemEstoque.AdicionarQuantidade(request.QuantidadeAdicionar, request.Valor);
        var financeiroResult = await _mediator.Send(new CreateTransacaoCommand
        {
            EmpresaId =  request.ItemId,
            Tipo = "Saida",
            Categoria = "Compras",
            Data = DateTime.UtcNow,
            Descricao = $"Compra de {request.QuantidadeAdicionar} {itemEstoque.UnidadeMedida.ToString()} de {itemEstoque.Nome}",
            Valor =  request.Valor
        });
        
        if (financeiroResult.IsFailed)
        {
            var listErros = financeiroResult.Errors.Select(e => e.Message).ToList();
            _logger.LogWarning("{UseCaseLogPrefix} Validação falhou na criação de uma nova transição por causa {Erros}", UseCaseLogPrefix, listErros);
            return Result.Fail(new ValidationError(listErros));
        }
        
        await _estoqueRepository.UpdateItemEstoqueAsync(itemEstoque);
        _logger.LogInformation("{UseCaseLogPrefix} Item de estoque atualizado com sucesso. EstoqueId: {EstoqueId}, QuantidadeAtual: {QuantidadeAtual}, ValorMedia: {ValorMedia}", UseCaseLogPrefix, itemEstoque.Id, itemEstoque.QuantidadeAtual, itemEstoque.ValorMedia);

        return Result.Ok();
    }
}
