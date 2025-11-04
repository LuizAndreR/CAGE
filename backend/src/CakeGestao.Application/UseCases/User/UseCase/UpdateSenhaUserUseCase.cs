using CakeGestao.Application.Dtos.Requests.Usuario;
using CakeGestao.Application.UseCases.User.Interface;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.User.UseCase;

public class UpdateSenhaUserUseCase : IUpdateSenhaUserUseCase
{
    private readonly ILogger<UpdateSenhaUserUseCase> _logger;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IValidator<UpdateSenhaUsuarioRequest> _validator;

    public UpdateSenhaUserUseCase(ILogger<UpdateSenhaUserUseCase> logger, IUsuarioRepository usuarioRepository, IValidator<UpdateSenhaUsuarioRequest> validator)
    {
        _logger = logger;
        _usuarioRepository = usuarioRepository;
        _validator = validator;
    }

    public Task<Result> ExecuteAsync(UpdateSenhaUsuarioRequest request, int id)
    {
        throw new NotImplementedException();
    }
}
