using CakeGestao.Application.Dtos.Responses;
using FluentResults;

namespace CakeGestao.Application.UseCases.Receitas.Interface;

public interface IGetReceitaUseCase
{
    public Task<Result<ReceitaResponse>> ExecuteAsync(int id);
}
