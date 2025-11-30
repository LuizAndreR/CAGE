using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Interfaces.Repositories;
using CakeGestao.Domain.Interfaces.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CakeGestao.Infrastructure.Security;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtTokenService> _logger;
    private readonly ITokenRepository _tokenRepository;

    public JwtTokenService(IConfiguration configuration, ILogger<JwtTokenService> logger, ITokenRepository tokenRepository)
    {
        _configuration = configuration;
        _logger = logger;
        _tokenRepository = tokenRepository;
    }

    public async Task<(string accessToken, string refreshToken)> TokenService(int usuarioId, string email, string role, int? empresaId)
    {
        _logger.LogInformation("Iniciando o processo de geração de tokens para o usuário {Id}", usuarioId);

        var accessToken = GenerateToken(usuarioId, email, role, empresaId);

        var refreshToken = await GenerateRefreshToken(usuarioId);

        _logger.LogInformation("Tokens gerados com sucesso para o usuário {Id}", usuarioId);
        return (accessToken, refreshToken);
    }

    private string GenerateToken(int usuarioId, string email, string role, int? empresaId)
    {
        _logger.LogInformation("Gerando token JWT para o usuário {Id}", usuarioId);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expirationMinutes = Convert.ToDouble(_configuration["Jwt:AccessTokenDurationMinutes"]);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role)
        };

        if (role != "Admin")
        {
            claims.Add(new Claim("EmpresaId", empresaId.ToString()!)); 
        }
        
        var token = new JwtSecurityToken(
           issuer: _configuration["Jwt:Issuer"],
           audience: _configuration["Jwt:Audience"],
           claims: claims,
           expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
           signingCredentials: creds);

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.WriteToken(token);

        _logger.LogInformation("Token JWT gerado com sucesso para o usuário {Id}", usuarioId);
        return jwtToken;
    }

    private async Task<string> GenerateRefreshToken(int usuarioId)
    {
        _logger.LogInformation("Iniciando geração de Refresh Token para o usuário {Id}", usuarioId);
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        var refreshToken = Convert.ToBase64String(randomNumber);

        var expirationDays = Convert.ToDouble(_configuration["Jwt:RefreshTokenDurationDays"]);
        var accessTokenExpiration = Convert.ToDouble(_configuration["Jwt:AccessTokenDurationMinutes"]);

        var tokenRefresh = new TokenRefresh
        {
            UsuarioId = usuarioId,
            RefreshToken = refreshToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
            IsRevoked = false,
            IsUsed = false
        };

        await _tokenRepository.SaveRefreshTokenAsync(tokenRefresh);
        _logger.LogInformation("Refresh Token gerado e salvo para o usuário {Id}", usuarioId);
        return refreshToken;
    }
}
