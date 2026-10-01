using CakeGestao.Application.Features.Auth.Login;
using CakeGestao.Application.Features.Auth.Refresh;
using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Exceptions;
using CakeGestao.Domain.Interfaces.Repositories;
using CakeGestao.Domain.Interfaces.Security;
using CakeGestao.Tests.Support;
using FluentResults;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CakeGestao.Tests.Commands;

public class AuthCommandTests
{
    [Theory]
    [InlineData("ausente")]
    [InlineData("senha_incorreta")]
    [InlineData("sucesso")]
    public async Task LoginSoEmiteTokensComCredenciaisValidas(string scenario)
    {
        var user = new Usuario("Ana", "ana@example.com", BCrypt.Net.BCrypt.HashPassword("senha123", workFactor: 4), UserRole.Dono, DateTime.UtcNow.AddDays(-1), 10) { Id = 7 };
        var repo = new Mock<IUsuarioRepository>();
        repo.Setup(r => r.GetUsuarioByEmailAsync(user.Email)).ReturnsAsync(scenario == "ausente" ? Result.Fail<Usuario>("Ausente") : Result.Ok(user));
        var jwt = new Mock<IJwtTokenService>(MockBehavior.Strict);
        if (scenario == "sucesso") jwt.Setup(j => j.TokenService(7, "Ana", "ana@example.com", "Dono", 10)).ReturnsAsync(("access-test", "refresh-test"));
        var result = await new LoginHandler(NullLogger<LoginHandler>.Instance, jwt.Object, repo.Object, new LoginUserValidator())
            .Handle(new() { Email = user.Email, Senha = scenario == "senha_incorreta" ? "outra123" : "senha123" }, default);
        Assert.Equal(scenario == "sucesso", result.IsSuccess);
        if (scenario == "sucesso") { Assert.Equal("access-test", result.Value.AccessToken); Assert.Equal("refresh-test", result.Value.RefreshToken); jwt.VerifyAll(); }
        else { Assert.IsType<ValidationError>(Assert.Single(result.Errors)); jwt.VerifyNoOtherCalls(); }
        repo.Verify(r => r.UpdateUsuarioAsync(user), scenario == "sucesso" ? Times.Once() : Times.Never());
    }

    [Fact]
    public async Task LoginFormatoInvalidoNaoConsultaBancoNemEmiteToken()
    {
        var repo = new Mock<IUsuarioRepository>(MockBehavior.Strict);
        var jwt = new Mock<IJwtTokenService>(MockBehavior.Strict);
        var result = await new LoginHandler(NullLogger<LoginHandler>.Instance, jwt.Object, repo.Object, new LoginUserValidator())
            .Handle(new() { Email = "invalido", Senha = "123" }, default);
        Assert.IsType<ValidationError>(Assert.Single(result.Errors)); repo.VerifyNoOtherCalls(); jwt.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("ausente")]
    [InlineData("expirado")]
    [InlineData("usado")]
    [InlineData("revogado")]
    public async Task RefreshInvalidoNaoConsultaUsuarioNemGeraSessao(string scenario)
    {
        var token = new TokenRefresh { RefreshToken = "antigo", UsuarioId = 7, IsUsed = scenario == "usado", IsRevoked = scenario == "revogado",
            ExpiresAt = scenario == "expirado" ? DateTime.UtcNow.AddDays(-1) : DateTime.UtcNow.AddDays(1) };
        var tokens = new Mock<ITokenRepository>(MockBehavior.Strict);
        tokens.Setup(r => r.GetRefreshTokenAsync("antigo")).ReturnsAsync(scenario == "ausente" ? Result.Fail<TokenRefresh>("Ausente") : Result.Ok(token));
        var users = new Mock<IUsuarioRepository>(MockBehavior.Strict);
        var jwt = new Mock<IJwtTokenService>(MockBehavior.Strict);
        var result = await new RefreshTokenHandler(NullLogger<RefreshTokenHandler>.Instance, tokens.Object, users.Object, jwt.Object)
            .Handle(new() { RefreshToken = "antigo" }, default);
        Assert.IsType<ValidationError>(Assert.Single(result.Errors)); users.VerifyNoOtherCalls(); jwt.VerifyNoOtherCalls();
        tokens.Verify(r => r.UpdateRefreshTokenAsync(It.IsAny<TokenRefresh>()), Times.Never);
    }

    [Fact]
    public async Task RefreshValidoRevogaTokenAnteriorAntesDeEmitirOutro()
    {
        var token = new TokenRefresh { RefreshToken = "antigo", UsuarioId = 7, ExpiresAt = DateTime.UtcNow.AddDays(1) };
        var tokens = new Mock<ITokenRepository>(MockBehavior.Strict);
        var users = new Mock<IUsuarioRepository>(MockBehavior.Strict);
        var jwt = new Mock<IJwtTokenService>(MockBehavior.Strict);
        var sequence = new MockSequence();
        tokens.InSequence(sequence).Setup(r => r.GetRefreshTokenAsync("antigo")).ReturnsAsync(Result.Ok(token));
        tokens.InSequence(sequence).Setup(r => r.UpdateRefreshTokenAsync(It.Is<TokenRefresh>(t => t.IsUsed && t.IsRevoked))).ReturnsAsync(Result.Ok());
        users.InSequence(sequence).Setup(r => r.GetByIdAsync(7, null)).ReturnsAsync(Result.Ok(TestData.Usuario()));
        jwt.InSequence(sequence).Setup(j => j.TokenService(7, "Ana", "ana@example.com", "Caixa", 10)).ReturnsAsync(("novo-access", "novo-refresh"));
        var result = await new RefreshTokenHandler(NullLogger<RefreshTokenHandler>.Instance, tokens.Object, users.Object, jwt.Object)
            .Handle(new() { RefreshToken = "antigo" }, default);
        Assert.Equal("novo-access", result.Value.AccessToken); Assert.Equal("novo-refresh", result.Value.RefreshToken);
        tokens.VerifyAll(); users.VerifyAll(); jwt.VerifyAll();
    }
}
