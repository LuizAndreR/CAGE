using CakeGestao.Application.Features.Financeiro.Command.Create;
using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Estoque.Command.Create;

public class CreateEstoqueHandler : IRequestHandler<CreateEstoqueCommand, Result>
{
    private readonly IEstoqueRepository _estoqueRepository;
    private readonly ILogger<CreateEstoqueHandler> _logger;
    private readonly IValidator<CreateEstoqueCommand> _validator;
    private readonly IMediator _mediator;
    private const string UseCaseLogPrefix = "[Create Estoque]";

    public CreateEstoqueHandler(IEstoqueRepository estoqueRepository, ILogger<CreateEstoqueHandler> logger, IValidator<CreateEstoqueCommand> validator, IMediator mediator)
    {
        _estoqueRepository = estoqueRepository;
        _logger = logger;
        _validator = validator;
        _mediator = mediator;   
    }

    public async Task<Result> Handle(CreateEstoqueCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{UseCaseLogPrefix} Iniciando processo de criação de item de estoque. EmpresaId: {EmpresaId}, Nome: {Nome}", UseCaseLogPrefix, request.EmpresaId, request.Nome);
        
        ValidationResult validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("{UseCaseLogPrefix} Validação falhou para novo item de estoque. EmpresaId: {EmpresaId}, Nome: {Nome}. Erros: {Errors}", UseCaseLogPrefix, request.EmpresaId, request.Nome, errors);
            return Result.Fail(new ValidationError(errors));
        }
        
        var existingItemResult = await _estoqueRepository.ExistItemByNome(request.Nome, request.EmpresaId);
        if(existingItemResult.IsSuccess)
        {
            _logger.LogWarning("{UseCaseLogPrefix} Já existe um item de estoque com o nome fornecido. EmpresaId: {EmpresaId}, Nome: {Nome}", UseCaseLogPrefix, request.EmpresaId, request.Nome);
            return Result.Fail(new ConflictError("Já existe um item de estoque com o nome fornecido."));
        }
        
        var unidadeEnum = Enum.Parse<UnidadeMedidaEnum>(request.UnidadeMedida, ignoreCase: true);
        var itemEstoque = new ItemEstoque(
                request.Nome,
                request.QuantidadeAtual,
                unidadeEnum,
                request.QunatidadeMinina,
                request.Valor,
                request.EmpresaId
        );
        
        var financeiroResult = await _mediator.Send(new CreateTransacaoCommand
        {
            EmpresaId =  request.EmpresaId,
            Tipo = "Saida",
            Categoria = "Compras",
            Data = DateTime.UtcNow,
            Descricao = $"Compra de {request.QuantidadeAtual} {itemEstoque.UnidadeMedida.ToString()} de {itemEstoque.Nome}",
            Valor =  request.Valor
        });
        
        if (financeiroResult.IsFailed)
        {
            var listErros = financeiroResult.Errors.Select(e => e.Message).ToList();
            _logger.LogWarning("{UseCaseLogPrefix} Validação falhou na criação de uma nova transição por causa {Erros}", UseCaseLogPrefix, listErros);
            return Result.Fail(new ValidationError(listErros));
        }

        await _estoqueRepository.CreateItemEstoqueAsync(itemEstoque);
        
        _logger.LogInformation("{UseCaseLogPrefix} Persistência concluída com sucesso. EmpresaId: {EmpresaId}, Nome: {Nome}, ItemEstoqueId: {ItemEstoqueId}", UseCaseLogPrefix, itemEstoque.EmpresaId, itemEstoque.Nome, itemEstoque.Id);

        return Result.Ok();
    }
}
