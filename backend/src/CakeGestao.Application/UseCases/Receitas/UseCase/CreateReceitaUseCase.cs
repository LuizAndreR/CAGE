using AutoMapper;
using CakeGestao.Application.Dtos.Requests.Receita;
using CakeGestao.Application.UseCases.Receitas.Interface;
using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.Receitas.UseCase;

public class CreateReceitaUseCase : ICreateReceitaUseCase
{
    private readonly IReceitaRepository _repository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateReceitaRequest> _validator;
    private readonly ILogger<CreateReceitaUseCase> _logger;

    public CreateReceitaUseCase(IReceitaRepository repository, IMapper mapper, IValidator<CreateReceitaRequest> validator, ILogger<CreateReceitaUseCase> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result> Execute(CreateReceitaRequest request)
    {
        _logger.LogInformation("Iniciando o processo de criação de uma nova receita de nome: {Nome}", request.Nome);
        
        _logger.LogInformation("Iniciando o processo de verificação da request de uma nova receita de nome: {Nome}", request.Nome);
        var resultValidato = _validator.Validate(request);
        if (!resultValidato.IsValid)
        {
            _logger.LogInformation("A varificação da request de criação de uma nova receita de nome: {Nome} falou",  request.Nome);
            return Result.Fail(new ValidationError(resultValidato.Errors.Select(e => e.ErrorMessage).ToList()));
        }
        _logger.LogInformation("Request de criação de uma nova receita de nome: {Nome} verificada com sucesso", request.Nome);

        _logger.LogInformation("Iniciando o processo de verificação de existência de receita de nome: {Nome}", request.Nome);
        var receitaExistsResult = await _repository.ExistReceitaAsync(request.Nome);
        if (receitaExistsResult.Value is true)
        {
            _logger.LogError("Erro ao verificar existência de receita de nome: {Nome}", request.Nome);
            return Result.Fail(new ConflictError($"Já existe uma receita cadastrada com o nome: {request.Nome}"));
        }
        _logger.LogInformation("Verificação de existência de receita de nome: {Nome} realizada com sucesso", request.Nome);

        _logger.LogInformation("Iniciando o mapeamento da request para a entidade de receita");
        var receita = _mapper.Map<Receita>(request);
        _logger.LogInformation("Mapeamento da request para a entidade de receita realizado com sucesso");

        await _repository.CreateReceitaAsync(receita);
        _logger.LogInformation("Receita de nome: {Nome} criada com sucesso", request.Nome);

        return Result.Ok();
    }
}