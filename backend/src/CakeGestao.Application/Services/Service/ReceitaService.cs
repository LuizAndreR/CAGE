using CakeGestao.Application.Dtos.Requests.Receita;
using CakeGestao.Application.Dtos.Responses;
using CakeGestao.Application.Services.Interface;
using CakeGestao.Application.UseCases.Receitas.Interface;
using FluentResults;

namespace CakeGestao.Application.Services.Service;

public class ReceitaService : IReceitaService
{
    private readonly ICreateReceitaUseCase _createReceitaUseCase;
    private readonly IGetReceitaUseCase _getReceitaUseCase;

    public ReceitaService(ICreateReceitaUseCase createReceitaUseCase, IGetReceitaUseCase getReceitaUseCase)
    {
        _createReceitaUseCase = createReceitaUseCase;
        _getReceitaUseCase = getReceitaUseCase;
    }

    public async Task<Result> CreateReceita(CreateReceitaRequest request) => await _createReceitaUseCase.Execute(request);
    public async Task<Result<ReceitaResponse>> GetReceita(int id) => await _getReceitaUseCase.ExecuteAsync(id);
}
