using CakeGestao.Application.Dtos.Requests.Empresa;
using CakeGestao.Application.Dtos.Responses;
using CakeGestao.Application.Services.Interface;
using CakeGestao.Application.UseCases.Empresas.Interface;
using FluentResults;

namespace CakeGestao.Application.Services.Service;

public class EmpresaService : IEmpresaService
{
    private readonly ICreateEmpresaUseCase _createEmpresaUseCase;
    private readonly IGetAllEmpresaUseCase _getAllEmpresaUseCase;
    private readonly IGetEmpresaUseCase _getEmpresaUseCase;
    private readonly IUpdateEmpresaUseCase _updateEmpresaUseCase;
    private readonly IDeleteEmpresaUseCase _deleteEmpresaUseCase;
    private readonly IUpdateStatusEmpresaUseCase _updateStatusEmpresaUseCase;

    public EmpresaService(ICreateEmpresaUseCase createEmpresaUseCase, IGetAllEmpresaUseCase getAllEmpresaUseCase, IGetEmpresaUseCase getEmpresaUseCase, IUpdateEmpresaUseCase updateEmpresaUseCase, IDeleteEmpresaUseCase deleteEmpresaUseCase, IUpdateStatusEmpresaUseCase updateStatusEmpresaUseCase)
    {
        _createEmpresaUseCase = createEmpresaUseCase;
        _getAllEmpresaUseCase = getAllEmpresaUseCase;
        _getEmpresaUseCase = getEmpresaUseCase;
        _updateEmpresaUseCase = updateEmpresaUseCase;
        _deleteEmpresaUseCase = deleteEmpresaUseCase;
        _updateStatusEmpresaUseCase = updateStatusEmpresaUseCase;
    }

    public async Task<Result> CreateAsync(CreateEmpresaRequest request) => await _createEmpresaUseCase.ExecuteAsync(request);
    public async Task<Result<List<EmpresaResponse>>> GetAllAsync() => await _getAllEmpresaUseCase.ExecuteAsync();
    public async Task<Result<EmpresaResponse>> GetByIdAsync(int id) => await _getEmpresaUseCase.ExecuteAsync(id);
    public async Task<Result> UpdateAsync(UpdateEmpresaRequest request, int id) => await _updateEmpresaUseCase.ExecuteAsync(request, id);
    public async Task<Result> DeleteAsync(int id) => await _deleteEmpresaUseCase.ExecuteAsync(id);
    public async Task<Result> UpdateStatusAsync(UpdateStatusEmpresaRequest request, int id) => await _updateStatusEmpresaUseCase.ExecuteAsync(request, id);
}
