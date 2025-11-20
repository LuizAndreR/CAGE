using CakeGestao.Application.Dtos.Requests.Empresa;
using FluentResults;

namespace CakeGestao.Application.UseCases.Empresas.Interface;

public interface IUpdateStatusEmpresaUseCase
{
    public Task<Result> ExecuteAsync(UpdateStatusEmpresaRequest request, int id);
}
