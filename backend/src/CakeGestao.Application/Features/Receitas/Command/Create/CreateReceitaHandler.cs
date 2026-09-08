using CakeGestao.Application.Common;
using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Exceptions;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Receitas.Command.Create;

public class CreateReceitaHandler : IRequestHandler<CreateReceitaCommand, Result>
{
    private readonly IReceitaRepository _receitaRepository;
    private readonly IValidator<CreateReceitaCommand> _validator;
    private readonly IEstoqueRepository _estoqueRepository;
    private readonly ILogger<CreateReceitaHandler> _logger;
    private const string LogPrefix = "[Create Receita Handler]";

    public CreateReceitaHandler(IReceitaRepository receitaRepository, IValidator<CreateReceitaCommand> validator, IEstoqueRepository estoqueRepository, ILogger<CreateReceitaHandler> logger)
    {
        _receitaRepository = receitaRepository;
        _validator = validator;
        _estoqueRepository = estoqueRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(CreateReceitaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Iniciando criação de receita. Nome: {Nome} | EmpresaId: {EmpresaId}", LogPrefix, request.Nome, request.EmpresaId);
        
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("{LogPrefix} Validação falhou. Nome: {Nome}. Erros: {Errors}", LogPrefix, request.Nome, string.Join(", ", errors));
            return Result.Fail(new ValidationError(errors));
        }

        Receita receita = new(request.Nome, request.ModoPreparo, request.EmpresaId);
        
        decimal custoDaReceitaIngrediente = 0;

        foreach (var dto in request.Ingredientes)
        {
            var resultItem = await _estoqueRepository.GetItemEstoqueByIdAsync(dto.ItemId, request.EmpresaId);

            if (resultItem.IsFailed)
                return Result.Fail($"Item de estoque ID {dto.ItemId} não encontrado.");

            Domain.Entities.Estoque itemEstoque = resultItem.Value;

            UnidadeMedidaEnum origemEnum = Enum.Parse<UnidadeMedidaEnum>(dto.UnidadeMedida, ignoreCase: true);
            var resultadoConversao = ConversorUnidade.Converter(
                quantidade: dto.Quantidade,
                origem: origemEnum, 
                destino: itemEstoque.UnidadeMedida,
                unidadeReferenciaVolume: itemEstoque.UnidadeReferenciaVolume,
                pesoReferenciaEmGramas: itemEstoque.PesoReferenciaEmGramas
            );
            if (resultadoConversao.IsFailed)
            {
                string mensagemErro = resultadoConversao.Errors.First().Message;
                _logger.LogWarning("{LogPrefix} Erro de conversão para o item {ItemId}: {Erro}", LogPrefix, dto.ItemId, mensagemErro);
                return Result.Fail(new ValidationError(mensagemErro));
            }

            decimal quantidadeConvertidaParaEstoque = resultadoConversao.Value;

            Ingrediente ingrediente = new Ingrediente
            (
                dto.ItemId,
                quantidadeConvertidaParaEstoque,
                itemEstoque.UnidadeMedida
            );

            receita.AdicionarIngrediente(ingrediente);

            decimal custoDesseIngrediente = itemEstoque.ValorMedia * quantidadeConvertidaParaEstoque;
            custoDaReceitaIngrediente += custoDesseIngrediente;

        }

        _logger.LogInformation("{Custo}, {Magem}, {extra}", custoDaReceitaIngrediente, request.PercentualMargemLucro, request.PercentualCustoExtra);

        receita.CalcularPrecificacao(
            custoIngredientes: custoDaReceitaIngrediente,
            percCustoExtra: request.PercentualCustoExtra,
            percMargemLucro: request.PercentualMargemLucro,
            precoVendaInformado: request.PrecoVenda
        );

        _logger.LogInformation("{Preco}, {Magem}, {extra}, {custo}", receita.PrecoVenda, receita.PercentualMargemLucro, receita.CustoTotal, custoDaReceitaIngrediente);
        
        _logger.LogInformation("{LogPrefix} Custo da receita '{Nome}' calculado: {CustoTotal}", LogPrefix, receita.Nome, custoDaReceitaIngrediente);
        
        await _receitaRepository.CreateReceitaAsync(receita);
        _logger.LogInformation("{LogPrefix} Receita criada com sucesso. Nome: {Nome}", LogPrefix, receita.Nome);

        return Result.Ok();
    }
}