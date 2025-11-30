using CakeGestao.Application.Dtos.Requests.Empresa;
using FluentResults;

namespace CakeGestao.Application.UseCases.Empresas.Interface;

public interface IUpdateEmpresaUseCase
{
    public Task<Result> ExecuteAsync(UpdateEmpresaRequest request, int id);
}
