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

    public EmpresaService(ICreateEmpresaUseCase createEmpresaUseCase, IGetAllEmpresaUseCase getAllEmpresaUseCase)
    {
        _createEmpresaUseCase = createEmpresaUseCase;
        _getAllEmpresaUseCase = getAllEmpresaUseCase;
    }

    public async Task<Result> CreateAsync(CreateEmpresaRequest request) => await _createEmpresaUseCase.ExecuteAsync(request);
    public async Task<Result<List<EmpresaResponse>>> GetAllAsync() => await _getAllEmpresaUseCase.ExecuteAsync();
}
