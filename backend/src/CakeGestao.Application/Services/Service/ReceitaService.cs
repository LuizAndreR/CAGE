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
    private readonly IGetAllReceitaUseCase _getAllReceitaUseCase;

    public ReceitaService(ICreateReceitaUseCase createReceitaUseCase, IGetReceitaUseCase getReceitaUseCase,  IGetAllReceitaUseCase getAllReceitaUseCase)
    {
        _createReceitaUseCase = createReceitaUseCase;
        _getReceitaUseCase = getReceitaUseCase;
        _getAllReceitaUseCase = getAllReceitaUseCase;
    }

    public async Task<Result> CreateReceita(CreateReceitaRequest request) => await _createReceitaUseCase.Execute(request);
    public async Task<Result<ReceitaResponse>> GetReceita(int id) => await _getReceitaUseCase.ExecuteAsync(id);
    public async Task<Result<List<ReceitaResponse>>> GetAllReceita() => await _getAllReceitaUseCase.Execute();
}
