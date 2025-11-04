using AutoMapper;
using CakeGestao.Application.Dtos.Requests.Auth;
using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Enun;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.Auth.Cadastro;

public class CadastroUseCase : ICadastroUseCase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ILogger<CadastroUseCase> _logger;
    private readonly IMapper _mapper;
    private readonly IValidator<CadastroRequest> _validator;

    public CadastroUseCase(IUsuarioRepository usuarioRepository, ILogger<CadastroUseCase> logger, IMapper mapper, IValidator<CadastroRequest> validator)
    {
        _usuarioRepository = usuarioRepository;
        _logger = logger;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<Result> Execute(CadastroRequest request)
    {
        _logger.LogInformation("Iniciando o processo de cadastro de um novo usuario {Email}", request.Email);

        _logger.LogInformation("Validando os dados do novo usuario {Email}", request.Email);
        var validationResult = _validator.Validate(request);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validação falhou para o novo usuario {Email}", request.Email);
            throw new ValidationError(validationResult.Errors.Select(e => e.ErrorMessage).ToList());
        }
        _logger.LogInformation("Validação realizada com sucesso para o novo usuario {Email}", request.Email);

        _logger.LogInformation("Verificando se role do novo usuario {Email} valido", request.Email);
        if (!Enum.TryParse<UserRole>(request.Role, true, out var userRole))
        {
            _logger.LogWarning("Role inválida fornecida para o novo usuario {Email}", request.Email);
            throw new ValidationError(new List<string> { "Role inválida" });
        }
        _logger.LogInformation("Role verificado com sucesso para o novo usuario {Email}", request.Email);

        _logger.LogInformation("Verificando se existencia do usuario {Email}", request.Email);
        var usuarioExistenteResult = await _usuarioRepository.GetUsuarioByEmailAsync(request.Email);
        if (usuarioExistenteResult.IsSuccess)
        {
            _logger.LogWarning("Usuario {Email} já existe no sistema", request.Email);
            throw new ConflictError("Usuário com mesmo email já existe");
        }
        _logger.LogInformation("Usuario {Email} não existe no sistema, prosseguindo com o cadastro", request.Email);

        _logger.LogInformation("Criptografando a senha do usuario {Email}", request.Email);
        var senhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha);
        _logger.LogInformation("Criptografia da senha realizada com sucesso para o usuario {Email}", request.Email);

        _logger.LogInformation("Iniciando o processo de mapeamento do novo usuario {Email}", request.Email);
        var usuario = _mapper.Map<Usuario>(request);
        usuario.SenhaHash = senhaHash;
        usuario.Role = userRole;
        _logger.LogInformation("Mapeamento realizado com sucesso para o novo usuario {Email}", request.Email);

        await _usuarioRepository.CreateUserAsync(usuario);
        return Result.Ok();
    }
}
