using FluentResults;

namespace CakeGestao.Application.UseCases.Empresas.Interface;

public interface IDeleteEmpresaUseCase
{
    public Task<Result> ExecuteAsync(int id);
}
