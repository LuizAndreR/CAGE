using AutoMapper;
using CakeGestao.Application.Dtos.Requests.Receita;
using CakeGestao.Application.UseCases.Receitas.Interface;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.Receitas.UseCase;

public class UpdateReceitaUseCase : IUpdateReceitaUseCase
{
    private readonly IReceitaRepository _receitaRepository;
    private readonly ILogger<UpdateReceitaUseCase> _logger;
    private readonly IValidator<UpdateReceitaRequest> _validator;
    private readonly IMapper _mapper;

    public UpdateReceitaUseCase(IReceitaRepository receitaRepository, ILogger<UpdateReceitaUseCase> logger, IValidator<UpdateReceitaRequest> validator, IMapper mapper)
    {
        _receitaRepository = receitaRepository;
        _logger = logger;
        _validator = validator;
        _mapper = mapper;
    }

    public async Task<Result> Execute(UpdateReceitaRequest request, int id)
    {
        _logger.LogInformation("Iniciando processo de atualização da receita: {Nome}", request.Nome);

        _logger.LogInformation("Validando dados da receita: {Nome}", request.Nome);
        var validationResult = _validator.Validate(request);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validação falhou para a receita: {Nome}", request.Nome);
            return Result.Fail(new ValidationError(validationResult.Errors.Select(e => e.ErrorMessage).ToList()));
        }
        _logger.LogInformation("Dados validados com sucesso para a receita: {Nome}", request.Nome);

        _logger.LogInformation("Varificando existência da receita: {Nome}", request.Nome);
        var existingReceitaResult = await _receitaRepository.GetReceitaByIdAsync(id);
        if (existingReceitaResult.IsFailed)
        {
            _logger.LogWarning("Receita não encontrada para atualização: {Nome}", request.Nome);
            return Result.Fail(new NotFoundError($"Receita com ID {id} não encontrada."));
        }
        _logger.LogInformation("Receita encontrada para atualização: {Nome}", request.Nome);

        _logger.LogInformation("Mapeando dados para atualização da receita: {Nome}", request.Nome);
        var receita = _mapper.Map(request, existingReceitaResult.Value);
        _logger.LogInformation("Dados mapeados com sucesso para a receita: {Nome}", request.Nome);

        var updateResult = await _receitaRepository.UpdateReceitaAsync(receita);
        _logger.LogInformation("Receita atualizada com sucesso: {Nome}", request.Nome);

        return Result.Ok();
    }
}
