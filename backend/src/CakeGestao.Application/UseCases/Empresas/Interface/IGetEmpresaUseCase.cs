using CakeGestao.Application.Dtos.Responses;
using FluentResults;

namespace CakeGestao.Application.UseCases.Empresas.Interface;

public interface IGetEmpresaUseCase
{
    public Task<Result<EmpresaResponse>> ExecuteAsync(int id);
}
