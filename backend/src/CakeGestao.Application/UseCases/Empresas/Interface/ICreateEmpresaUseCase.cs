using CakeGestao.Application.Dtos.Requests.Empresa;
using FluentResults;

namespace CakeGestao.Application.UseCases.Empresas.Interface;

public interface ICreateEmpresaUseCase
{
    public Task<Result> ExecuteAsync(CreateEmpresaRequest request);
}
