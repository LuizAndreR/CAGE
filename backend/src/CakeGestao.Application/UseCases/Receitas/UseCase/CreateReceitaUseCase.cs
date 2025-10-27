using AutoMapper;
using CakeGestao.Application.Dtos.Requests.Receita;
using CakeGestao.Application.UseCases.Receitas.Interface;
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

    public Task<Result> Execute(CreateReceitaRequest request)
    {
        _logger.LogInformation("Iniciando o processo de criação de uma nova receita de nome: {Nome}", request.Nome);
        
        _logger.LogInformation("Iniciando o processo de verificação da request de uma nova receita de nome: {Nome}", request.Nome);
        var resultValidato = _validator.Validate(request);
        if (!resultValidato.IsValid)
        {
            _logger.LogInformation("A varificação da request de criação de uma nova receita de nome: {Nome} falou",  request.Nome);
        }
    }
}