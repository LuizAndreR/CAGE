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
    private readonly IGetItemEstoqueUseCase _getItemEstoqueUseCase;
    private readonly IDeleteItemEstoqueUseCase _deleteItemEstoqueUseCase;
    private readonly IUpdateItemEstoqueUseCase _updateItemEstoqueUseCase;

    public EstoqueService(ICreateEstoqueUseCase createEmpresaUseCase, IAddQuantidadeEstoqueUseCase addQuantidadeEstoqueUseCase, IGetAllItemEstoqueUseCase getAllItemEstoqueUseCase, IGetItemEstoqueUseCase getItemEstoqueUseCase, IDeleteItemEstoqueUseCase deleteItemEstoqueUseCase, IUpdateItemEstoqueUseCase updateItemEstoqueUseCase)
    {
        _createEmpresaUseCase = createEmpresaUseCase;
        _addQuantidadeEstoqueUseCase = addQuantidadeEstoqueUseCase;
        _getAllItemEstoqueUseCase = getAllItemEstoqueUseCase;
        _getItemEstoqueUseCase = getItemEstoqueUseCase;
        _deleteItemEstoqueUseCase = deleteItemEstoqueUseCase;
        _updateItemEstoqueUseCase = updateItemEstoqueUseCase;
    }

    public async Task<Result> CreateEstoqueAsync(CreateEstoqueRequest request) => await _createEmpresaUseCase.ExecuteAsync(request);
    public async Task<Result> AddQuantidadeEstoqueAsync(AddQuantidadeEstoqueRequest request) => await _addQuantidadeEstoqueUseCase.ExecuteAsync(request);
    public async Task<Result<List<ItemEstoqueResponse>>> GetAllItemEstoque(ItemEstoqueRequest request) => await _getAllItemEstoqueUseCase.ExecuteAsync(request);
    public async Task<Result<ItemEstoqueResponse>> GetItemEstoqueById(ItemEstoqueRequest request) => await _getItemEstoqueUseCase.ExecuteAsync(request);
    public async Task<Result> DeleteItemEstoqueAsync(ItemEstoqueRequest request) => await _deleteItemEstoqueUseCase.ExecuteAsync(request);
    public async Task<Result> UpdateItemEstoqueAsync(UpdateItemEstoqueRequest request) => await _updateItemEstoqueUseCase.ExecuteAsync(request);
}
