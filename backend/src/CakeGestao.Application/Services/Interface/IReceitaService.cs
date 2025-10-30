using CakeGestao.Application.Dtos.Requests.Receita;
using CakeGestao.Application.Dtos.Responses;
using FluentResults;

namespace CakeGestao.Application.Services.Interface;

public interface IReceitaService
{
    public Task<Result> CreateReceita(CreateReceitaRequest request);
    public Task<Result<ReceitaResponse>> GetReceita(int id);
}
