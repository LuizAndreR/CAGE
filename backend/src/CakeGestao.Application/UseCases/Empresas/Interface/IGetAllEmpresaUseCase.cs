using CakeGestao.Application.Dtos.Responses;
using CakeGestao.Domain.Entities;
using FluentResults;

namespace CakeGestao.Application.UseCases.Empresas.Interface;

public interface IGetAllEmpresaUseCase
{
    public Task<Result<List<EmpresaResponse>>> ExecuteAsync();
}