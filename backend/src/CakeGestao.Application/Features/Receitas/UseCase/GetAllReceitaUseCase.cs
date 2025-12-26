using AutoMapper;
using CakeGestao.Application.Dtos.Responses;
using CakeGestao.Application.UseCases.Receitas.Interface;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.Receitas.UseCase;

public class GetAllReceitaUseCase : IGetAllReceitaUseCase
{
    private readonly IReceitaRepository _receitaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllReceitaUseCase> _logger;

    public GetAllReceitaUseCase(IReceitaRepository receitaRepository, IMapper mapper, ILogger<GetAllReceitaUseCase> logger)
    {
        _receitaRepository = receitaRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<ReceitaResponse>>> Execute()
    {
        _logger.LogInformation("Iniciando o processo de get de todas as receita do banco de dados");

        var receitasResult = await _receitaRepository.GetAllReceitasAsync();

        if (receitasResult.IsFailed)
        {
            _logger.LogInformation("Nenhuma receita foi encontrada.");
            return Result.Fail(new NotFoundError("Nenhuma receita foi encontrada no banco de dados."));
        }
        _logger.LogInformation("Busca realizada com susseso, foi encontrado {Numero} receitas cadastrada no banco de dados.", receitasResult.Value.Count);
        
        _logger.LogInformation("Incinado o processo de mapeamento da lista de receitas");
        var listReceitas = _mapper.Map<List<ReceitaResponse>>(receitasResult.Value);
        _logger.LogInformation("Processo de mapeamento realizado com susseso ");
        
        _logger.LogInformation("Finalizado o processo de get all de todas receitas no banco de dados");
        return Result.Ok(listReceitas);    
    }
}