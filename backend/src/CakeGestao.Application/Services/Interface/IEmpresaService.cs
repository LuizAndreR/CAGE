using CakeGestao.Application.Dtos.Requests.Empresa;
using CakeGestao.Application.Dtos.Responses;
using FluentResults;

namespace CakeGestao.Application.Services.Interface;

public interface IEmpresaService
{
    public Task<Result<List<EmpresaResponse>>> GetAllAsync();
    public Task<Result> CreateAsync(CreateEmpresaRequest request);
}
