using CakeGestao.Application.Dtos.Requests.Receita;
using FluentResults;

namespace CakeGestao.Application.Services.Interface;

public interface IReceitaService
{
    public Task<Result> CreateReceita(CreateReceitaRequest request);
}
