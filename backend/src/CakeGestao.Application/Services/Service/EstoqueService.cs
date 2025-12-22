using CakeGestao.Application.Dtos.Requests.Estoque;
using CakeGestao.Application.Services.Interface;
using CakeGestao.Application.UseCases.Estoque.Interface;
using FluentResults;

namespace CakeGestao.Application.Services.Service;

public class EstoqueService : IEstoqueService
{
    private readonly ICreateEstoqueUseCase _createEmpresaUseCase;
    private readonly IAddQuantidadeEstoqueUseCase _addQuantidadeEstoqueUseCase;

    public EstoqueService(ICreateEstoqueUseCase createEmpresaUseCase, IAddQuantidadeEstoqueUseCase addQuantidadeEstoqueUseCase)
    {
        _createEmpresaUseCase = createEmpresaUseCase;
        _addQuantidadeEstoqueUseCase = addQuantidadeEstoqueUseCase;
    }

    public async Task<Result> CreateEstoqueAsync(CreateEstoqueRequest request) => await _createEmpresaUseCase.ExecuteAsync(request);
    public async Task<Result> AddQuantidadeEstoqueAsync(AddQuantidadeEstoqueRequest request) => await _addQuantidadeEstoqueUseCase.ExecuteAsync(request);
}
