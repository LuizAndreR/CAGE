using AutoMapper;
using CakeGestao.Application.Common;
using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Receitas.Command.Update;

public class UpdateReceitaHandler: IRequestHandler<UpdateReceitaCommand, Result>
{
    private readonly IReceitaRepository _receitaRepository;
    private readonly IEstoqueRepository _estoqueRepository;
    private readonly ILogger<UpdateReceitaHandler> _logger;
    private readonly IValidator<UpdateReceitaCommand> _validator;
    private const string LogPrefix = "[Update Receita Handler]";

    public UpdateReceitaHandler(IReceitaRepository receitaRepository, IEstoqueRepository estoqueRepository, ILogger<UpdateReceitaHandler> logger, IValidator<UpdateReceitaCommand> validator)
    {
        _receitaRepository = receitaRepository;
        _estoqueRepository = estoqueRepository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result> Handle(UpdateReceitaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Iniciando atualização. Receita ID: {Id} | EmpresaId: {EmpresaId}", LogPrefix, request.Id, request.EmpresaId);

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("{LogPrefix} Validação falhou. Erros: {Erros}", LogPrefix, string.Join(", ", errors));
            return Result.Fail(new ValidationError(errors));
        }

        var existingReceitaResult = await _receitaRepository.GetReceitaByIdAsync(request.Id, request.EmpresaId);
        if (existingReceitaResult.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Receita ID {Id} não encontrada ou não pertence à empresa {EmpresaId}.", LogPrefix, request.Id, request.EmpresaId);
            return Result.Fail(new NotFoundError($"Receita com ID {request.Id} não encontrada."));
        }
        Receita receita = existingReceitaResult.Value;

        receita.AtualizarReceita(request.Nome, request.ModoPreparo, request.PrecoVenda);
        if (request.Ingredientes != null && request.Ingredientes.Any())
        {
            receita.LimparIngredientes();
            decimal novoCustoTotalDaReceita = 0;

            foreach (var dto in request.Ingredientes)
            {
                var resultItem = await _estoqueRepository.GetItemEstoqueByIdAsync(dto.ItemId, request.EmpresaId);
                if (resultItem.IsFailed) return Result.Fail($"Item ID {dto.ItemId} não encontrado.");

                var itemEstoque = resultItem.Value;
                var origemEnum = Enum.Parse<UnidadeMedidaEnum>(dto.UnidadeMedida, ignoreCase: true);

                var resultadoConversao = ConversorUnidade.Converter(
                    dto.Quantidade, origemEnum, itemEstoque.UnidadeMedida,
                    itemEstoque.UnidadeReferenciaVolume, itemEstoque.PesoReferenciaEmGramas
                );

                if (resultadoConversao.IsFailed) return Result.Fail(resultadoConversao.Errors);

                var quantidadeConvertida = resultadoConversao.Value;
                receita.AdicionarIngrediente(new Ingrediente(dto.ItemId, quantidadeConvertida, itemEstoque.UnidadeMedida));

                novoCustoTotalDaReceita += itemEstoque.ValorMedia * quantidadeConvertida;
            }

            receita.AtualizarCustoTotal(novoCustoTotalDaReceita);
            _logger.LogInformation("{LogPrefix} Ingredientes e custo atualizados. Novo Custo: {Custo}", LogPrefix, novoCustoTotalDaReceita);
        }
        else
        {
            _logger.LogInformation("{LogPrefix} Front-end não enviou ingredientes. Atualizando apenas textos.", LogPrefix);
        }

        await _receitaRepository.UpdateReceitaAsync(receita);
        _logger.LogInformation("{LogPrefix} Receita '{Nome}' atualizada com sucesso.", LogPrefix, receita.Nome);
        return Result.Ok();
    }
}
