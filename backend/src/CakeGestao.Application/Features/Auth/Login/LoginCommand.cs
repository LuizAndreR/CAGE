using CakeGestao.Application.Features.Auth.Common;
using FluentResults;
using MediatR;

namespace CakeGestao.Application.Features.Auth.Login;

public class LoginCommand : IRequest<Result<TokensResponse>>
{
    public required string Email { get; set; }
    public required string Senha { get; set; }
}
