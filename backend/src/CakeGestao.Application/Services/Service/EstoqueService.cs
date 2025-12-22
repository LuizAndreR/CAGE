using CakeGestao.Application.Dtos.Requests.Estoque;
using CakeGestao.Application.Dtos.Responses;
using CakeGestao.Application.Services.Interface;
using CakeGestao.Application.UseCases.Estoque.Interface;
using FluentResults;

namespace CakeGestao.Application.Services.Service;

public class EstoqueService : IEstoqueService
{
    private readonly ICreateEstoqueUseCase _createEmpresaUseCase;
    private readonly IAddQuantidadeEstoqueUseCase _addQuantidadeEstoqueUseCase;
    private readonly IGetAllItemEstoqueUseCase _getAllItemEstoqueUseCase;

    public EstoqueService(ICreateEstoqueUseCase createEmpresaUseCase, IAddQuantidadeEstoqueUseCase addQuantidadeEstoqueUseCase, IGetAllItemEstoqueUseCase getAllItemEstoqueUseCase)
    {
        _createEmpresaUseCase = createEmpresaUseCase;
        _addQuantidadeEstoqueUseCase = addQuantidadeEstoqueUseCase;
        _getAllItemEstoqueUseCase = getAllItemEstoqueUseCase;
    }

    public async Task<Result> CreateEstoqueAsync(CreateEstoqueRequest request) => await _createEmpresaUseCase.ExecuteAsync(request);
    public async Task<Result> AddQuantidadeEstoqueAsync(AddQuantidadeEstoqueRequest request) => await _addQuantidadeEstoqueUseCase.ExecuteAsync(request);
    public async Task<Result<List<ItemEstoqueResponse>>> GetAllItemEstoque(int empresaId) => await _getAllItemEstoqueUseCase.ExecuteAsync(empresaId);
}
