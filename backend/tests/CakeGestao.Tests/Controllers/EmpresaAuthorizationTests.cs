using CakeGestao.Application.Features.Empresas.Query.Get;
using CakeGestao.Application.Features.Empresas.Common;
using System.Reflection;
using System.Security.Claims;
using CakeGestao.API.Controllers;
using CakeGestao.Application.Features.Empresas.Command.Update;
using CakeGestao.Tests.Support;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CakeGestao.Tests.Controllers;

public class EmpresaAuthorizationTests
{
    public static IEnumerable<object[]> Permissoes()
    {
        var methods = new[] { nameof(EmpresaController.GetAllEmpresas), nameof(EmpresaController.GetEmpresaById),
            nameof(EmpresaController.CreateEmpresa), nameof(EmpresaController.UpdateEmpresa),
            nameof(EmpresaController.UpdateStatusEmpresa), nameof(EmpresaController.DeleteEmpresa), nameof(EmpresaController.UpdateEmpresaDono), nameof(EmpresaController.GetMinhaEmpresa) };
        foreach (var method in methods)
        foreach (var role in new[] { "Admin", "Dono", "Caixa", "Anonimo" })
            yield return new object[] { method, role, role == ((method == nameof(EmpresaController.UpdateEmpresaDono) || method == nameof(EmpresaController.GetMinhaEmpresa)) ? "Dono" : "Admin") };
    }

    [Theory]
    [MemberData(nameof(Permissoes))]
    public async Task PoliticaCombinadaDoControllerEActionRespeitaPermissoes(string method, string role, bool permitido)
    {
        var services = new ServiceCollection(); services.AddLogging(); services.AddAuthorization();
        using var provider = services.BuildServiceProvider();
        var metadata = typeof(EmpresaController).GetCustomAttributes<AuthorizeAttribute>(true)
            .Concat(typeof(EmpresaController).GetMethod(method)!.GetCustomAttributes<AuthorizeAttribute>(true));
        var policy = await AuthorizationPolicy.CombineAsync(provider.GetRequiredService<IAuthorizationPolicyProvider>(), metadata);
        Assert.NotNull(policy);
        var user = role == "Anonimo" ? new ClaimsPrincipal(new ClaimsIdentity()) : TestData.Context(role: role).HttpContext.User;
        var result = await provider.GetRequiredService<IAuthorizationService>().AuthorizeAsync(user, null, policy!);
        Assert.Equal(permitido, result.Succeeded);
    }

    [Fact]
    public async Task DonoConsultaSomenteEmpresaDoToken()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetEmpresaQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<EmpresaResponse>("Empresa não encontrada"));
        var controller = new EmpresaController(mediator.Object, NullLogger<EmpresaController>.Instance) { ControllerContext = TestData.Context() };
        await controller.GetMinhaEmpresa();
        mediator.Verify(m => m.Send(It.Is<GetEmpresaQuery>(q => q.Id == 10), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DonoAtualizaSomenteEmpresaDoToken()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<UpdateEmpresaCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Ok());
        var controller = new EmpresaController(mediator.Object, NullLogger<EmpresaController>.Instance) { ControllerContext = TestData.Context() };
        var request = new UpdateEmpresaCommand { Id = 99, Nome = "Empresa", Endereco = "Rua A" };
        Assert.IsType<OkObjectResult>(await controller.UpdateEmpresaDono(request));
        mediator.Verify(m => m.Send(It.Is<UpdateEmpresaCommand>(c => c.Id == 10), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("invalido")]
    [InlineData("0")]
    [InlineData("-1")]
    public async Task EmpresaInvalidaNoTokenNaoEnviaAtualizacao(string? empresa)
    {
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        var controller = new EmpresaController(mediator.Object, NullLogger<EmpresaController>.Instance) { ControllerContext = TestData.Context(empresa) };
        Assert.IsType<UnauthorizedObjectResult>(await controller.UpdateEmpresaDono(new() { Id = 99, Nome = "Empresa", Endereco = "Rua A" }));
        Assert.IsType<UnauthorizedObjectResult>(await controller.GetMinhaEmpresa());
        mediator.VerifyNoOtherCalls();
    }
}
