using AutoMapper;
using CakeGestao.Application.Dtos.Responses;
using CakeGestao.Application.UseCases.Receitas.Interface;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.Receitas.UseCase;

public class GetReceitaUseCase : IGetReceitaUseCase
{
    private readonly ILogger<GetReceitaUseCase> _logger;
    private readonly IReceitaRepository _receitaRepository;
    private readonly IMapper _mapper;

    public GetReceitaUseCase(ILogger<GetReceitaUseCase> logger, IReceitaRepository receitaRepository, IMapper mapper)
    {
        _logger = logger;
        _receitaRepository = receitaRepository;
        _mapper = mapper;
    }

    public async Task<Result<ReceitaResponse>> ExecuteAsync(int id)
    {
        _logger.LogInformation("Iniciando o processo de busca da receita de ID: {Id} no banco de dados", id);

        var resultRepository = await _receitaRepository.GetReceitaByIdAsync(id);
        if (resultRepository.IsFailed)
        {
            _logger.LogError("Receita de id: {Id} não encontrado no banco de dados", id);
            return Result.Fail(new NotFoundError($"Receita de id: {id} não encontrado no banco de dados"));
        }
        _logger.LogInformation("Receita de ID: {Id} encontrada com sucesso no banco de dados", id);

        _logger.LogInformation("Iniciando o processo de mapeamento da receita de ID: {Id}", id);
        var receitaResponse = _mapper.Map<ReceitaResponse>(resultRepository.Value);
        _logger.LogInformation("Mapeamento da receita de ID: {Id} realizado com sucesso", id);

        _logger.LogInformation("Processo de busca da receita de ID: {Id} concluído com sucesso", id);
        return Result.Ok(receitaResponse);
    }
}
