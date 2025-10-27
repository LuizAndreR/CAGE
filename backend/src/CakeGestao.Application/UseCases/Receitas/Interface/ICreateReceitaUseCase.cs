using CakeGestao.Application.Dtos.Requests.Receita;
using CakeGestao.Domain.Entities;
using FluentResults;

namespace CakeGestao.Application.UseCases.Receitas.Interface;

public interface ICreateReceitaUseCase
{
    public Task<Result> Execute(CreateReceitaRequest request);
}