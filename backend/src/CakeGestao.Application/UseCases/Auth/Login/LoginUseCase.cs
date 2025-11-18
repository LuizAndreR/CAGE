using AutoMapper;
using CakeGestao.Application.Dtos.Requests.Auth;
using CakeGestao.Application.Dtos.Responses;
using CakeGestao.Domain.Interfaces.Repositories;
using CakeGestao.Domain.Interfaces.Security;
using FluentResults;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.Auth.Login;

public class LoginUseCase : ILoginUseCase
{ 
    private readonly ILogger<LoginUseCase> _logger;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<LoginRequest> _validator;

    public LoginUseCase(ILogger<LoginUseCase> logger, IJwtTokenService jwtTokenService, IUsuarioRepository usuarioRepository, IMapper mapper, IValidator<LoginRequest> validator)
    {
        _logger = logger;
        _jwtTokenService = jwtTokenService;
        _usuarioRepository = usuarioRepository;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<Result<TokensResponce>> Execute(LoginRequest request)
    {
       _logger.LogInformation("Iniciando o processo de login para o usuario {Email}", request.Email);   

        _logger.LogInformation("Validando os dados de login para o usuario {Email}", request.Email);
        var validationResult = _validator.Validate(request);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validação falhou para o login do usuario {Email}", request.Email);
            var listErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result.Fail(new ValidationError(listErrors));
        }
        _logger.LogInformation("Validação realizada com sucesso para o login do usuario {Email}", request.Email);

        _logger.LogInformation("Buscando o usuario {Email} no sistema", request.Email);
        var usuarioResult = await _usuarioRepository.GetUsuarioByEmailAsync(request.Email);
        if (usuarioResult.IsFailed)
        {
            _logger.LogWarning("Usuario {Email} não encontrado no sistema", request.Email);
            return Result.Fail(new ValidationError(new List<string> { "Email ou senha inválidos" }));
        }
        _logger.LogInformation("Usuario {Email} encontrado no sistema", request.Email);

        _logger.LogInformation("Verificando a senha do usuario {Email}", request.Email);
        var usuario = usuarioResult.Value;
        if (!BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
        {
            _logger.LogWarning("Senha inválida para o usuario {Email}", request.Email);
            return Result.Fail(new ValidationError(new List<string> { "Email ou senha inválidos" }));
        }
        _logger.LogInformation("Senha verificada com sucesso para o usuario {Email}", request.Email);

        _logger.LogInformation("Gerando tokens de acesso e refresh token para o usuario {Email}", request.Email);
        var tokens = await _jwtTokenService.TokenService(usuario.Id, usuario.Email, usuario.Role.ToString(), usuario.EmpresaId);
        _logger.LogInformation("Tokens gerados com sucesso para o usuario {Email}", request.Email);

        _logger.LogInformation("Inciando o processo de atualização do ultimo login do usuario {Email}", request.Email); 
        usuario.UltimoLogin = DateTime.UtcNow;
        await _usuarioRepository.UpdateUsuarioAsync(usuario);
        _logger.LogInformation("Ultimo login atualizado com sucesso para o usuario {Email}", request.Email);

        _logger.LogInformation("Mapeando TokensResponce com os tokens do usuario {Email}", request.Email);
        var tokenResponse = new TokensResponce
        {
            AccessToken = tokens.accessToken,
            RefreshToken = tokens.refreshToken
        };
        _logger.LogInformation("Mapeamento do TokensResponce realizado com sucesso para o usuario {Email}", request.Email);

        _logger.LogInformation("Processo de login realizado com susseso");
        return Result.Ok(tokenResponse);
    }
}
