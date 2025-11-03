using CakeGestao.Application.Dtos.Requests.Auth;
using FluentResults;

namespace CakeGestao.Application.UseCases.Auth.Cadastro;

public interface ICadastroUseCase
{
    public Task<Result> Execute(CadastroRequest request);
}
