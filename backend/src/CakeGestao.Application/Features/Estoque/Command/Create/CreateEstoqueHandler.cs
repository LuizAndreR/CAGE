using CakeGestao.Application.Features.Financeiro.Command.Create;
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
    private const string LogPrefix = "[Create Estoque Handler]";

    public CreateEstoqueHandler(IEstoqueRepository estoqueRepository, ILogger<CreateEstoqueHandler> logger, IValidator<CreateEstoqueCommand> validator, IMediator mediator)
    {
        _estoqueRepository = estoqueRepository;
        _logger = logger;
        _validator = validator;
        _mediator = mediator;   
    }

    public async Task<Result> Handle(CreateEstoqueCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Iniciando cadastro de novo item. EmpresaId: {EmpresaId} | Nome: {Nome}", LogPrefix, request.EmpresaId, request.Nome);

        ValidationResult validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("{LogPrefix} Dados inválidos. EmpresaId: {EmpresaId}. Erros: {Errors}", LogPrefix, request.EmpresaId, string.Join(", ", errors));
            return Result.Fail(new ValidationError(errors));
        }
        
        var existingItemResult = await _estoqueRepository.ExistItemByNome(request.Nome, request.EmpresaId, request.Marca);
        if(existingItemResult.IsSuccess)
        {
            _logger.LogWarning("{LogPrefix} Item já cadastrado. EmpresaId: {EmpresaId} | Nome: {Nome}", LogPrefix, request.EmpresaId, request.Nome);
            return Result.Fail(new ConflictError("Já existe um item de estoque com o nome fornecido."));
        }
        
        var itemEstoque = new Domain.Entities.Estoque(
                request.Nome,
                request.Marca,
                request.QuantidadeAtual,
                request.UnidadeMedida != null ? Enum.Parse<UnidadeMedidaEnum>(request.UnidadeMedida, ignoreCase: true) : throw new ArgumentException("Unidade de medida é obrigatória."),
                request.QuantidadeMinima,
                request.Valor,
                request.EmpresaId,
                request.UnidadeMedidaReferenciaVolume != null ? global::System.Enum.Parse<global::CakeGestao.Domain.Enum.UnidadeMedidaEnum>(request.UnidadeMedidaReferenciaVolume, ignoreCase: true) : null,
                request.PesoReferenciaEmGramas
        );

        await _estoqueRepository.CreateItemEstoqueAsync(itemEstoque);

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
            var listErros = string.Join("; ", financeiroResult.Errors.Select(e => e.Message));
            _logger.LogWarning("{LogPrefix} Falha ao registrar financeiro. Cadastro abortado. Erros: {Errors}", LogPrefix, listErros);
            return Result.Fail(new ValidationError(new List<string> { "Falha ao registrar a despesa financeira do novo item." }));
        }

        _logger.LogInformation("{LogPrefix} Item criado com sucesso. ID Gerado: {Id} | Nome: {Nome}", LogPrefix, itemEstoque.Id, itemEstoque.Nome);
        return Result.Ok();
    }
}
