using CakeGestao.Application.Dtos.Requests.Empresa;
using CakeGestao.Application.Services.Interface;
using CakeGestao.Application.UseCases.Empresas.Interface;
using FluentResults;

namespace CakeGestao.Application.Services.Service;

public class EmpresaService : IEmpresaService
{
    private readonly ICreateEmpresaUseCase _createEmpresaUseCase;

    public EmpresaService(ICreateEmpresaUseCase createEmpresaUseCase)
    {
        _createEmpresaUseCase = createEmpresaUseCase;
    }

    public async Task<Result> CreateAsync(CreateEmpresaRequest request) => await _createEmpresaUseCase.ExecuteAsync(request);
}
