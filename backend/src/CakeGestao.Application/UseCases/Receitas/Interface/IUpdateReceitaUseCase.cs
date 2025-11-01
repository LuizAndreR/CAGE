using CakeGestao.Application.Dtos.Requests.Receita;
using FluentResults;

namespace CakeGestao.Application.UseCases.Receitas.Interface;

internal interface IUpdateReceitaUseCase
{
   public Task<Result> Execute(UpdateReceitaRequest request, int id);
}
