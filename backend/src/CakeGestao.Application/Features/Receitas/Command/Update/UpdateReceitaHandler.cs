using CakeGestao.Application.Common;
using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Exceptions;
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
        _logger.LogInformation("{LogPrefix} Iniciando atualização. Receita ID: {Id} | EmpresaId: {EmpresaId}",
            LogPrefix, request.Id, request.EmpresaId);

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
            _logger.LogWarning("{LogPrefix} Receita ID {Id} não encontrada ou não pertence à empresa {EmpresaId}.",
                LogPrefix, request.Id, request.EmpresaId);
            return Result.Fail(new NotFoundError($"Receita com ID {request.Id} não encontrada."));
        }

        Receita receita = existingReceitaResult.Value;
        
        receita.AtualizarReceita(request.Nome, request.ModoPreparo);

        decimal custoIngredientesParaCalculo = receita.PrecoIngredientes; 

        if (request.Ingredientes != null && request.Ingredientes.Any())
        {
            var verificacaoResult = await ProcessarEVerificarIngredientesAsync(receita, request.Ingredientes, request.EmpresaId);
            if (verificacaoResult.IsFailed) return verificacaoResult.ToResult();

            var (houveMudanca, novoCustoTotal) = verificacaoResult.Value;

            if (houveMudanca)
            {
                custoIngredientesParaCalculo = novoCustoTotal;
                _logger.LogInformation("{LogPrefix} Ingredientes alterados. Novo Custo: {Custo}", LogPrefix, custoIngredientesParaCalculo);
            }
            else
            {
                _logger.LogInformation("{LogPrefix} Ingredientes idênticos. Mantendo custo: {Custo}", LogPrefix, custoIngredientesParaCalculo);
            }
        }

        receita.CalcularPrecificacao(
            custoIngredientes: custoIngredientesParaCalculo,
            ValorCustoExtra: request.CustoExtra,
            percMargemLucro: request.PercentualMargemLucro,
            precoVendaInformado: request.PrecoVenda
        );

        await _receitaRepository.UpdateReceitaAsync(receita);
        _logger.LogInformation("{LogPrefix} Receita '{Nome}' atualizada com sucesso.", LogPrefix, receita.Nome);
        
        return Result.Ok();
    }
    
    private async Task<Result<(bool HouveMudanca, decimal NovoCustoTotal)>> ProcessarEVerificarIngredientesAsync(
        Receita receita, 
        List<UpdateIngredienteDto> ingredientesRequest, 
        int empresaId)
    {
        var ingredientesConvertidos = new List<Ingrediente>();
        decimal custoTotalCalculado = 0;

        var itemIds = ingredientesRequest.Select(i => i.ItemId).Distinct().ToList();
        var itensEstoqueResult = await _estoqueRepository.GetItensByIdsAsync(itemIds, empresaId);
        
        if (itensEstoqueResult.IsFailed) 
            return Result.Fail(itensEstoqueResult.Errors);

        var dictItens = itensEstoqueResult.Value.ToDictionary(i => i.Id);

        foreach (var dto in ingredientesRequest)
        {
            if (!dictItens.TryGetValue(dto.ItemId, out var estoque))
            {
                _logger.LogWarning("{LogPrefix} Item ID {ItemId} não encontrado no estoque.", LogPrefix, dto.ItemId);
                return Result.Fail(new NotFoundError($"Item ID {dto.ItemId} não encontrado no estoque."));
            }

            if (!Enum.TryParse<UnidadeMedidaEnum>(dto.UnidadeMedida, true, out var origemEnum))
                return Result.Fail(new ValidationError($"Unidade de medida inválida: {dto.UnidadeMedida}"));

            var resultadoConversao = ConversorUnidade.Converter(
                dto.Quantidade, origemEnum, estoque.UnidadeMedida,
                estoque.UnidadeReferenciaVolume, estoque.PesoReferenciaEmGramas
            );

            if (resultadoConversao.IsFailed)
                return Result.Fail(new ValidationError(resultadoConversao.Errors.First().Message));

            var quantidadeConvertida = resultadoConversao.Value;
            ingredientesConvertidos.Add(new Ingrediente(dto.ItemId, quantidadeConvertida, estoque.UnidadeMedida));
            
            custoTotalCalculado += estoque.ValorMedia * quantidadeConvertida;
        }

        bool houveMudanca = !SaoListasIguais(receita.Ingredientes, ingredientesConvertidos);

        if (houveMudanca)
        {
            receita.LimparIngredientes();
            
            foreach (var novoIng in ingredientesConvertidos)
            {
                receita.AdicionarIngrediente(novoIng);
            }
            
            return Result.Ok((true, custoTotalCalculado));
        }

        return Result.Ok((false, 0m));
    }
    
    private bool SaoListasIguais(IReadOnlyCollection<Ingrediente> banco, List<Ingrediente> convertidos)
    {
        if (banco == null || banco.Count != convertidos.Count) return false;

        var bancoOrdenado = banco.OrderBy(i => i.ItemId).ToList();
        var convertidosOrdenados = convertidos.OrderBy(i => i.ItemId).ToList();

        for (int i = 0; i < bancoOrdenado.Count; i++)
        {
            if (bancoOrdenado[i].ItemId != convertidosOrdenados[i].ItemId || 
                bancoOrdenado[i].Quantidade != convertidosOrdenados[i].Quantidade)
            {
                return false;
            }
        }
        return true;
    }
}

