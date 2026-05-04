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
    private const string LogPrefix = "[Update Item Estoque Handler]";

    public UpdateItemEstoqueHandler(IEstoqueRepository estoqueRepository, IValidator<UpdateItemEstoqueCommand> validator, ILogger<UpdateItemEstoqueHandler> logger)
    {
        _estoqueRepository = estoqueRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateItemEstoqueCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Iniciando atualização cadastral de item. ID: {ItemId}", LogPrefix, request.ItemId);

        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("{LogPrefix} Dados inválidos. ID: {ItemId}. Erros: {Errors}", LogPrefix, request.ItemId, string.Join(", ", errors));
            return Result.Fail(new ValidationError(errors));
        }

        var itemEstoqueResult = await _estoqueRepository.GetItemEstoqueByIdAsync(request.ItemId, request.EmpresaId);
        if (itemEstoqueResult.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Item não encontrado. ID: {ItemId}", LogPrefix, request.ItemId);
            return Result.Fail(new NotFoundError("Item de estoque não encontrado para atualização."));
        }
        var itemEstoque = itemEstoqueResult.Value;        

        itemEstoque.AtualizarDadosCadastrais(
            request.Nome,
            request.Marca,
            request.QuantidadeAtual,
            request.QuantidadeMinima,
            request.UnidadeMedida != null ? Enum.Parse<UnidadeMedidaEnum>(request.UnidadeMedida, ignoreCase: true) : throw new ArgumentException("Unidade de medida é obrigatória."),
            request.UnidadeMedidaReferenciaVolume != null ? Enum.Parse<UnidadeMedidaEnum>(request.UnidadeMedidaReferenciaVolume, ignoreCase: true) : null,
            request.PesoReferenciaEmGramas
        );

        await _estoqueRepository.UpdateItemEstoqueAsync(itemEstoque);

        _logger.LogInformation("{LogPrefix} Item atualizado com sucesso. ID: {ItemId} | Nome: {Nome}", LogPrefix, request.ItemId, itemEstoque.Nome);
        return Result.Ok();
    }
}
