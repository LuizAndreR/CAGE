using CakeGestao.Application.Features.Auth.Common;
using FluentResults;
using MediatR;

namespace CakeGestao.Application.Features.Auth.Refresh;

public class RefreshTokenCommand : IRequest<Result<TokensResponse>>
{
    public required string RefreshToken { get; set; }
}
