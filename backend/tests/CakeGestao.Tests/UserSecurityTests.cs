using System.Security.Claims;
using System.Text.Json;
using CakeGestao.API.Controllers;
using CakeGestao.Application.Features.Auth.Cadastro;
using CakeGestao.Application.Features.User.Command.Delete;
using CakeGestao.Application.Features.User.Command.UpdateFuncionario;
using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Exceptions;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CakeGestao.Tests;

public class UserSecurityTests
{
    private static ControllerContext Context(string role, string? empresa = "10")
    {
        var claims = new List<Claim> { new(ClaimTypes.Role, role) };
        if (empresa != null) claims.Add(new("EmpresaId", empresa));
        return new ControllerContext { HttpContext = new DefaultHttpContext {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test")) } };
    }

    private static Mock<IEmpresaRepository> Empresas()
    {
        var repo = new Mock<IEmpresaRepository>();
        repo.Setup(r => r.GetEmpresaByIdAsync(10))
            .ReturnsAsync(Result.Ok(new Empresa("Empresa", "Endereço", default) { Id = 10 }));
        return repo;
    }

    private static Usuario Usuario(UserRole role) => new("Original", "a@b.com", "hash", role, DateTime.UtcNow, 10) { Id = 7 };
    private static CadastroCommand Cadastro(string role) => new() {
        Nome = "Ana", Email = "ana@example.com", Senha = "senha123", Role = role, EmpresaId = 10 };

    [Fact]
    public void CadastroAdministrativoExigeAdmin()
    {
        var method = typeof(AuthContoller).GetMethod(nameof(AuthContoller.CadastroDono))!;
        var policy = Assert.Single(method.GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>());
        Assert.Equal("Admin", policy.Roles);
        Assert.Empty(method.GetCustomAttributes(typeof(AllowAnonymousAttribute), true));
        Assert.Empty(typeof(AuthContoller).GetCustomAttributes(typeof(AllowAnonymousAttribute), true));
    }

    [Fact]
    public void JsonNaoPodeDefinirPrivilegiosOuEmpresaDoCadastro()
    {
        var request = JsonSerializer.Deserialize<CadastroCommand>("""
            {"nome":"Ana","email":"ana@example.com","senha":"senha123","role":"Admin","adminRole":true,"empresaId":99}
            """, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        Assert.False(request.AdminRole);
        Assert.Equal(0, request.EmpresaId);
        var update = JsonSerializer.Deserialize<UpdateFuncionarioCommand>("""
            {"nome":"Ana","role":"Admin","adminRole":true,"empresaId":99,"id":99}
            """, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        Assert.False(update.AdminRole);
        Assert.Equal(0, update.EmpresaId);
        Assert.Equal(0, update.Id);
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("admin")]
    public async Task DonoNaoPodeCadastrarAdmin(string role)
    {
        var repo = new Mock<IUsuarioRepository>(MockBehavior.Strict);
        var handler = new CadastroHandler(repo.Object, NullLogger<CadastroHandler>.Instance, new CadastroUserValidator(Empresas().Object));
        var result = await handler.Handle(Cadastro(role), default);
        Assert.IsType<ForbiddenError>(Assert.Single(result.Errors));
        repo.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("999")]
    [InlineData("Admin, Dono")]
    public async Task CadastroRejeitaRolesNumericasOuCompostas(string role)
    {
        var result = await new CadastroUserValidator(Empresas().Object).ValidateAsync(Cadastro(role));
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("Confeiteiro", false)]
    [InlineData("caixa", false)]
    [InlineData("Admin", true)]
    [InlineData("Dono", true)]
    [InlineData("Dono", false)]
    [InlineData("dono", false)]
    public async Task CadastroLegitimoContinuaFuncionando(string role, bool admin)
    {
        var repo = new Mock<IUsuarioRepository>();
        repo.Setup(r => r.GetUsuarioByEmailAsync(It.IsAny<string>())).ReturnsAsync(Result.Fail<Usuario>("Não encontrado"));
        var request = Cadastro(role);
        request.AdminRole = admin;
        var handler = new CadastroHandler(repo.Object, NullLogger<CadastroHandler>.Instance, new CadastroUserValidator(Empresas().Object));
        Assert.True((await handler.Handle(request, default)).IsSuccess);
        repo.Verify(r => r.CreateUserAsync(It.Is<Usuario>(u => u.EmpresaId == 10 && u.Role == Enum.Parse<UserRole>(role, true))), Times.Once);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ControllerSobrescreveFlagAdministrativaDoCadastro(bool administrativeRoute)
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<CadastroCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Ok());
        var controller = new AuthContoller(mediator.Object, NullLogger<AuthContoller>.Instance) {
            ControllerContext = Context(administrativeRoute ? "Admin" : "Dono") };
        var request = Cadastro("Dono");
        request.AdminRole = !administrativeRoute;
        request.EmpresaId = 99;
        if (administrativeRoute) await controller.CadastroDono(request, 10);
        else await controller.Cadastro(request);
        Assert.Equal(administrativeRoute, request.AdminRole);
        Assert.Equal(10, request.EmpresaId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DonoNaoPodeSelecionarOutraEmpresa(bool delete)
    {
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        var controller = new UserController(NullLogger<UserController>.Instance, mediator.Object) { ControllerContext = Context("Dono") };
        IActionResult result = delete ? await controller.DeleteUsuario(7, 99)
            : await controller.UpdateFuncaoUsuario(new() { Nome = "Ana", Role = "Caixa" }, 7, 99);
        Assert.IsType<ForbidResult>(result);
        mediator.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("invalido")]
    public async Task EmpresaNaQueryNaoSubstituiTokenInvalido(string? empresa)
    {
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        var controller = new UserController(NullLogger<UserController>.Instance, mediator.Object) { ControllerContext = Context("Dono", empresa) };
        Assert.IsType<UnauthorizedObjectResult>(await controller.DeleteUsuario(7, 99));
        Assert.IsType<UnauthorizedObjectResult>(await controller.UpdateFuncaoUsuario(new() { Nome = "Ana", Role = "Caixa" }, 7, 99));
        mediator.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("Dono", 0, 10, false)]
    [InlineData("Dono", 10, 10, false)]
    [InlineData("Admin", 99, 99, true)]
    public async Task EmpresaEPrivilegioDerivamDoAtor(string role, int query, int expected, bool admin)
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<UpdateFuncionarioCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Ok());
        mediator.Setup(m => m.Send(It.IsAny<DeleteUsuarioCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Ok());
        var controller = new UserController(NullLogger<UserController>.Instance, mediator.Object) { ControllerContext = Context(role, admin ? null : "10") };
        await controller.UpdateFuncaoUsuario(new() { Nome = "Ana", Role = "Caixa", AdminRole = !admin, EmpresaId = 99 }, 7, query);
        await controller.DeleteUsuario(7, query);
        mediator.Verify(m => m.Send(It.Is<UpdateFuncionarioCommand>(r => r.EmpresaId == expected && r.AdminRole == admin && r.Id == 7), It.IsAny<CancellationToken>()), Times.Once);
        mediator.Verify(m => m.Send(It.Is<DeleteUsuarioCommand>(r => r.EmpresaId == expected && r.AdminRole == admin && r.Id == 7), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(UserRole.Caixa, "Admin")]
    [InlineData(UserRole.Caixa, "admin")]
    [InlineData(UserRole.Caixa, "Dono")]
    [InlineData(UserRole.Admin, "Caixa")]
    [InlineData(UserRole.Dono, "Caixa")]
    public async Task DonoNaoPodePromoverNemAlterarContaPrivilegiada(UserRole original, string desired)
    {
        var usuario = Usuario(original);
        var repo = new Mock<IUsuarioRepository>();
        repo.Setup(r => r.GetByIdAsync(7, 10)).ReturnsAsync(Result.Ok(usuario));
        var handler = new UpdateFuncionarioHandler(NullLogger<UpdateFuncionarioHandler>.Instance, repo.Object, new UpdateFuncionarioValidator());
        var result = await handler.Handle(new() { Id = 7, EmpresaId = 10, Nome = "Alterado", Role = desired }, default);
        Assert.IsType<ForbiddenError>(Assert.Single(result.Errors));
        Assert.Equal(original, usuario.Role);
        Assert.Equal("Original", usuario.Nome);
        repo.Verify(r => r.UpdateUsuarioAsync(It.IsAny<Usuario>()), Times.Never);
    }

    [Theory]
    [InlineData(false, "caixa")]
    [InlineData(true, "Admin")]
    [InlineData(true, "Dono")]
    public async Task AlteracoesAutorizadasPersistem(bool admin, string role)
    {
        var repo = new Mock<IUsuarioRepository>();
        repo.Setup(r => r.GetByIdAsync(7, 10)).ReturnsAsync(Result.Ok(Usuario(UserRole.Caixa)));
        var handler = new UpdateFuncionarioHandler(NullLogger<UpdateFuncionarioHandler>.Instance, repo.Object, new UpdateFuncionarioValidator());
        Assert.True((await handler.Handle(new() { Id = 7, EmpresaId = 10, Nome = "Ana", Role = role, AdminRole = admin }, default)).IsSuccess);
        repo.Verify(r => r.UpdateUsuarioAsync(It.Is<Usuario>(u => u.Role == Enum.Parse<UserRole>(role, true))), Times.Once);
    }

    [Theory]
    [InlineData(UserRole.Admin, false, false)]
    [InlineData(UserRole.Dono, false, false)]
    [InlineData(UserRole.Caixa, false, true)]
    [InlineData(UserRole.Dono, true, true)]
    public async Task ExclusaoRespeitaPrivilegios(UserRole role, bool admin, bool allowed)
    {
        var repo = new Mock<IUsuarioRepository>();
        repo.Setup(r => r.GetByIdAsync(7, 10)).ReturnsAsync(Result.Ok(Usuario(role)));
        var handler = new DeleteUsuarioHandler(repo.Object, NullLogger<DeleteUsuarioHandler>.Instance, new DeleteUsuarioValidator(Empresas().Object));
        var result = await handler.Handle(new() { Id = 7, EmpresaId = 10, AdminRole = admin }, default);
        Assert.Equal(allowed, result.IsSuccess);
        if (!allowed) Assert.IsType<ForbiddenError>(Assert.Single(result.Errors));
        repo.Verify(r => r.DeleteAsync(It.IsAny<Usuario>()), allowed ? Times.Once() : Times.Never());
    }

    [Fact]
    public async Task IdDeOutraEmpresaNaoPodeSerAlteradoOuExcluido()
    {
        var repo = new Mock<IUsuarioRepository>();
        repo.Setup(r => r.GetByIdAsync(99, 10)).ReturnsAsync(Result.Fail<Usuario>("Não encontrado"));
        var update = new UpdateFuncionarioHandler(NullLogger<UpdateFuncionarioHandler>.Instance, repo.Object, new UpdateFuncionarioValidator());
        var delete = new DeleteUsuarioHandler(repo.Object, NullLogger<DeleteUsuarioHandler>.Instance, new DeleteUsuarioValidator(Empresas().Object));
        Assert.IsType<NotFoundError>(Assert.Single((await update.Handle(new() { Id = 99, EmpresaId = 10, Nome = "Ana", Role = "Caixa" }, default)).Errors));
        Assert.IsType<NotFoundError>(Assert.Single((await delete.Handle(new() { Id = 99, EmpresaId = 10 }, default)).Errors));
        repo.Verify(r => r.UpdateUsuarioAsync(It.IsAny<Usuario>()), Times.Never);
        repo.Verify(r => r.DeleteAsync(It.IsAny<Usuario>()), Times.Never);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RepositorioRealPreservaUsuarioDeOutraEmpresa(bool delete)
    {
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<CakeGestao.Infrastructure.Data.CageContext>();
        Microsoft.EntityFrameworkCore.InMemoryDbContextOptionsExtensions.UseInMemoryDatabase(options, Guid.NewGuid().ToString());
        await using var db = new CakeGestao.Infrastructure.Data.CageContext(options.Options);
        var foreign = new Usuario("Outra empresa", "foreign@example.com", "hash", UserRole.Caixa, DateTime.UtcNow, 20) { Id = 99 };
        db.Usuarios.Add(foreign);
        await db.SaveChangesAsync();
        var repo = new CakeGestao.Infrastructure.Data.Repositories.UsuarioRepository(db,
            NullLogger<CakeGestao.Infrastructure.Data.Repositories.UsuarioRepository>.Instance);
        Result result;
        if (delete)
            result = await new DeleteUsuarioHandler(repo, NullLogger<DeleteUsuarioHandler>.Instance, new DeleteUsuarioValidator(Empresas().Object))
                .Handle(new() { Id = 99, EmpresaId = 10 }, default);
        else
            result = await new UpdateFuncionarioHandler(NullLogger<UpdateFuncionarioHandler>.Instance, repo, new UpdateFuncionarioValidator())
                .Handle(new() { Id = 99, EmpresaId = 10, Nome = "Invadido", Role = "Atendente" }, default);
        Assert.IsType<NotFoundError>(Assert.Single(result.Errors));
        db.ChangeTracker.Clear();
        var persisted = await db.Usuarios.FindAsync(99);
        Assert.NotNull(persisted);
        Assert.Equal("Outra empresa", persisted.Nome);
        Assert.Equal(UserRole.Caixa, persisted.Role);
        Assert.Equal(20, persisted.EmpresaId);
    }
}
