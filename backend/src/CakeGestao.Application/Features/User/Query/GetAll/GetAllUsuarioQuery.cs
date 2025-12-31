using CakeGestao.Application.Features.User.Common;
using FluentResults;
using MediatR;

namespace CakeGestao.Application.Features.User.Query.GetAll;

public class GetAllUsuarioQuery : IRequest<Result<List<UsuarioResponse>>>
{
    
}