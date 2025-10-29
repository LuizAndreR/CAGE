using CakeGestao.Application.Dtos.Requests.Receita;
using CakeGestao.Application.Services.Interface;
using CakeGestao.Application.UseCases.Receitas.Interface;
using FluentResults;

namespace CakeGestao.Application.Services.Service;

public class ReceitaService : IReceitaService
{
    private readonly ICreateReceitaUseCase _createReceitaUseCase;

    public ReceitaService(ICreateReceitaUseCase createReceitaUseCase)
    {
        _createReceitaUseCase = createReceitaUseCase;
    }

    public async Task<Result> CreateReceita(CreateReceitaRequest request) => await _createReceitaUseCase.Execute(request);
}
