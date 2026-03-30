using CakeGestao.Domain.Entities;
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

        Receita receita = new(request.Nome, request.ModoPreparo, request.PrecoVenda, request.EmpresaId);
        
        decimal custoTotalDaReceita = 0;

        foreach (var dto in request.Ingredientes)
        {
            Ingrediente ingrediente = new
            (
                dto.ItemId,
                dto.Quantidade,
                dto.UnidadeMedida
            );
            
            receita.AdicionarIngrediente(ingrediente);
            
            var resultItem = await _estoqueRepository.GetItemEstoqueByIdAsync(dto.ItemId, request.EmpresaId);
            var itemEstoque = resultItem.Value;
            var custoDesseIngrediente = itemEstoque.ValorMedia * dto.Quantidade;
            custoTotalDaReceita += custoDesseIngrediente;
            
        }
        
         receita.AtualizarCustoTotal(custoTotalDaReceita);
        _logger.LogInformation("{LogPrefix} Custo da receita '{Nome}' calculado: {CustoTotal}", LogPrefix, receita.Nome, custoTotalDaReceita);
        
        await _receitaRepository.CreateReceitaAsync(receita);
        _logger.LogInformation("{LogPrefix} Receita criada com sucesso. Nome: {Nome}", LogPrefix, receita.Nome);

        return Result.Ok();
    }
}