using AutoMapper;
using CakeGestao.Application.Dtos.Responses;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Receitas.Query.GetReceita;

public class GetReceitaHandler : IRequestHandler<GetReceitaQuery, Result<ReceitaResponse>>
{
    private readonly ILogger<GetReceitaHandler> _logger;
    private readonly IReceitaRepository _receitaRepository;
    private readonly IMapper _mapper;

    public GetReceitaHandler(ILogger<GetReceitaHandler> logger, IReceitaRepository receitaRepository, IMapper mapper)
    {
        _logger = logger;
        _receitaRepository = receitaRepository;
        _mapper = mapper;
    }

    public async Task<Result<ReceitaResponse>> Handle(GetReceitaQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando o processo de busca da receita de ID: {Id} no banco de dados", request.Id);

        var resultRepository = await _receitaRepository.GetReceitaByIdAsync(request.Id);
        if (resultRepository.IsFailed)
        {
            _logger.LogError("Receita de id: {Id} não encontrado no banco de dados", request.Id);
            return Result.Fail(new NotFoundError($"Receita de id: {request.Id} não encontrado no banco de dados"));
        }
        _logger.LogInformation("Receita de ID: {Id} encontrada com sucesso no banco de dados", request.Id);

        _logger.LogInformation("Iniciando o processo de mapeamento da receita de ID: {Id}", request.Id);
        var receitaResponse = _mapper.Map<ReceitaResponse>(resultRepository.Value);
        _logger.LogInformation("Mapeamento da receita de ID: {Id} realizado com sucesso", request.Id);

        _logger.LogInformation("Processo de busca da receita de ID: {Id} concluído com sucesso", request.Id);
        return Result.Ok(receitaResponse);
    }
}
