using CakeGestao.Application.Dtos.Requests.Empresa;
using FluentResults;

namespace CakeGestao.Application.Services.Interface;

public interface IEmpresaService
{
    public Task<Result> CreateAsync(CreateEmpresaRequest request);
}
