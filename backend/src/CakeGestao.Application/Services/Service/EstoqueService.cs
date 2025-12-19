using CakeGestao.Application.Dtos.Requests.Estoque;
using CakeGestao.Application.Services.Interface;
using CakeGestao.Application.UseCases.Estoque.Interface;
using FluentResults;

namespace CakeGestao.Application.Services.Service;

public class EstoqueService : IEstoqueService
{
    private readonly ICreateEstoqueUseCase _createEmpresaUseCase;

    public EstoqueService(ICreateEstoqueUseCase createEmpresaUseCase)
    {
        _createEmpresaUseCase = createEmpresaUseCase;
    }

    public async Task<Result> CreateEstoqueAsync(CreateEstoqueRequest request) => await _createEmpresaUseCase.ExecuteAsync(request);
}
