using CakeGestao.Application.Dtos.Responses;
using FluentResults;

namespace CakeGestao.Application.UseCases.Receitas.Interface;

public interface IGetAllReceitaUseCase
{
    public Task<Result<List<ReceitaResponse>>> Execute();
}