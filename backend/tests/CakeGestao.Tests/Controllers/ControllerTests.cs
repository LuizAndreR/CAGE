using CakeGestao.API.Controllers;
using CakeGestao.Application.Features.Auth.Common;
using CakeGestao.Application.Features.Auth.Login;
using CakeGestao.Application.Features.Auth.Refresh;
using CakeGestao.Application.Features.Empresas.Command.Update;
using CakeGestao.Application.Features.Estoque.Command.AddQuantidade;
using CakeGestao.Application.Features.Estoque.Command.RemoverQuantidade;
using CakeGestao.Application.Features.Estoque.Command.Delete;
using CakeGestao.Application.Features.Receitas.Command.Status;
using CakeGestao.Application.Features.Pedidos.Command.Create;
using CakeGestao.Application.Features.Pedidos.Command.UpdateStatus;
using CakeGestao.Application.Features.Financeiro.Command.Cancelamento;
using CakeGestao.Application.Features.Financeiro.Query.GetAll;
using CakeGestao.Application.Features.Financeiro.Common;
using CakeGestao.Application.Features.Dashboard.Query;
using CakeGestao.Application.Features.Dashboard.Common;
using CakeGestao.Application.Features.User.Query.Get;
using CakeGestao.Application.Features.User.Common;
using CakeGestao.Domain.Exceptions;
using CakeGestao.Tests.Support;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CakeGestao.Tests.Controllers;

public class ControllerTests
{
    private readonly Mock<IMediator> _mediator = new(MockBehavior.Strict);
    private object? _sent;
    private void CommandResult(Result result) => _mediator
        .Setup(m => m.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>()))
        .Callback<IRequest<Result>, CancellationToken>((request, _) => _sent = request).ReturnsAsync(result);
    private void QueryResult<T>(Result<T> result) => _mediator
        .Setup(m => m.Send(It.IsAny<IRequest<Result<T>>>(), It.IsAny<CancellationToken>()))
        .Callback<IRequest<Result<T>>, CancellationToken>((request, _) => _sent = request).ReturnsAsync(result);

    [Theory]
    [InlineData("validation", 400)]
    [InlineData("forbidden", 403)]
    [InlineData("conflict", 409)]
    [InlineData("missing", 404)]
    [InlineData("unexpected", 500)]
    public async Task ControllerConverteErroDoHandlerParaStatusHttp(string kind, int status)
    {
        Error error = kind switch {
            "validation" => new ValidationError("Dados inválidos"), "forbidden" => new ForbiddenError("Sem acesso"),
            "conflict" => new ConflictError("Duplicado"), "missing" => new NotFoundError("Não encontrado"), _ => new Error("Falha") };
        CommandResult(Result.Fail(error));
        var controller = new EstoqueController(_mediator.Object, NullLogger<EstoqueController>.Instance) { ControllerContext = TestData.Context() };
        var response = Assert.IsAssignableFrom<ObjectResult>(await controller.DeleteItemEstoque(3));
        Assert.Equal(status, response.StatusCode);
        var sent = Assert.IsType<DeleteItemEstoqueCommand>(_sent);
        Assert.Equal(3, sent.ItemId);
        Assert.Equal(10, sent.EmpresaId);
    }

    [Theory]
    [InlineData("Estoque")]
    [InlineData("Receita")]
    [InlineData("Pedido")]
    [InlineData("Financeiro")]
    public async Task ConsultasSemEmpresaRetornam401SemEnviarAoMediator(string feature)
    {
        Func<Task<IActionResult>>[] actions = feature switch {
            "Estoque" => EstoqueQueries(), "Receita" => ReceitaQueries(), "Pedido" => PedidoQueries(), _ => FinanceiroQueries() };
        foreach (var action in actions) Assert.Equal(401, Assert.IsAssignableFrom<IStatusCodeActionResult>(await action()).StatusCode);
        _mediator.VerifyNoOtherCalls();
    }
    private Func<Task<IActionResult>>[] EstoqueQueries()
    {
        var c = new EstoqueController(_mediator.Object, NullLogger<EstoqueController>.Instance) { ControllerContext = TestData.Context(null) };
        return new Func<Task<IActionResult>>[] { c.GetAllEstoqueAsync, c.GetEstoqueAlertAsync, () => c.GetEstoqueByIdAsync(1) };
    }
    private Func<Task<IActionResult>>[] ReceitaQueries()
    {
        var c = new ReceitaController(NullLogger<ReceitaController>.Instance, _mediator.Object) { ControllerContext = TestData.Context(null) };
        return new Func<Task<IActionResult>>[] { c.GetAllReceitas, () => c.GetReceitaById(1) };
    }
    private Func<Task<IActionResult>>[] PedidoQueries()
    {
        var c = new PedidoController(NullLogger<FinanceiroController>.Instance, _mediator.Object) { ControllerContext = TestData.Context(null) };
        return new Func<Task<IActionResult>>[] { c.GetAll, () => c.GetPedidoById(1) };
    }
    private Func<Task<IActionResult>>[] FinanceiroQueries()
    {
        var c = new FinanceiroController(NullLogger<FinanceiroController>.Instance, _mediator.Object) { ControllerContext = TestData.Context(null) };
        return new Func<Task<IActionResult>>[] { c.GetResumo, () => c.GetById(1), () => c.GetAll(null, null, null, null) };
    }

    [Fact]
    public async Task EstoqueEntradaESaidaUsamEmpresaDoTokenEIdDaRota()
    {
        CommandResult(Result.Ok());
        var c = new EstoqueController(_mediator.Object, NullLogger<EstoqueController>.Instance) { ControllerContext = TestData.Context() };
        var entrada = new AddQuantidadeEstoqueCommand { EmpresaId = 99, ItemId = 99, QuantidadeAdicionar = 2, Valor = 40 };
        Assert.IsType<OkObjectResult>(await c.AddQuantidadeEstoque(3, entrada));
        Assert.Equal(10, entrada.EmpresaId); Assert.Equal(3, entrada.ItemId); Assert.Equal(40, entrada.Valor);
        var saida = new RemoveQuantidadeEstoqueCommand { EmpresaId = 99, ItemId = 99, QuantidadeARemover = 2 };
        Assert.IsType<OkObjectResult>(await c.RemoveQuantidadeEstoque(3, saida));
        Assert.Equal(10, saida.EmpresaId); Assert.Equal(3, saida.ItemId);
        _mediator.Verify(m => m.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ReceitaStatusUsaEmpresaDoTokenEIdDaRota()
    {
        CommandResult(Result.Ok());
        var c = new ReceitaController(NullLogger<ReceitaController>.Instance, _mediator.Object) { ControllerContext = TestData.Context() };
        var request = new UpdateReceitaStatusCommand { EmpresaId = 99, ReceitaId = 99, Status = false };
        Assert.IsType<OkObjectResult>(await c.UpdateStatus(3, request));
        Assert.Same(request, _sent); Assert.Equal(10, request.EmpresaId); Assert.Equal(3, request.ReceitaId);
    }

    [Fact]
    public async Task PedidoCriacaoEStatusUsamEmpresaDoToken()
    {
        CommandResult(Result.Ok());
        var c = new PedidoController(NullLogger<FinanceiroController>.Instance, _mediator.Object) { ControllerContext = TestData.Context() };
        var create = new CreatePedidoCommand { EmpresaId = 99, ClienteNome = "Cliente" };
        Assert.IsType<OkObjectResult>(await c.CreatePedido(create)); Assert.Equal(10, create.EmpresaId);
        var status = new UpdateStatusPedidoCommand { EmpresaId = 99, PedidoId = 99, Status = "EmProducao" };
        Assert.IsType<OkObjectResult>(await c.UpdateStatus(status, 3));
        Assert.Equal(10, status.EmpresaId); Assert.Equal(3, status.PedidoId);
    }

    [Fact]
    public async Task FinanceiroCancelamentoUsaUsuarioEmpresaERota()
    {
        CommandResult(Result.Ok());
        var c = new FinanceiroController(NullLogger<FinanceiroController>.Instance, _mediator.Object) { ControllerContext = TestData.Context() };
        var request = new CancelTransacaoCommand { EmpresaId = 99, UsuarioId = 99, TransacaoId = 99, Motivo = "Duplicada" };
        Assert.IsType<OkObjectResult>(await c.CancelarTransacao(request, 3));
        Assert.Equal(10, request.EmpresaId); Assert.Equal(7, request.UsuarioId); Assert.Equal(3, request.TransacaoId);
    }

    [Fact]
    public async Task FinanceiroEncaminhaFiltrosEDevolvePayload()
    {
        var payload = new List<TransacaoResponse>();
        QueryResult(Result.Ok(payload));
        var c = new FinanceiroController(NullLogger<FinanceiroController>.Instance, _mediator.Object) { ControllerContext = TestData.Context() };
        Assert.Same(payload, Assert.IsType<OkObjectResult>(await c.GetAll("Saida", "Compras", 5, 2026)).Value);
        var query = Assert.IsType<GetAllTransacaoQuery>(_sent);
        Assert.Equal(10, query.EmpresaId); Assert.Equal("Saida", query.Tipo); Assert.Equal("Compras", query.Categoria);
        Assert.Equal(5, query.Mes); Assert.Equal(2026, query.Ano);
    }

    [Fact]
    public async Task EmpresaAtualizacaoUsaRota()
    {
        CommandResult(Result.Ok());
        var c = new EmpresaController(_mediator.Object, NullLogger<EmpresaController>.Instance) { ControllerContext = TestData.Context(role: "Admin") };
        var request = new UpdateEmpresaCommand { Id = 99, Nome = "Novo nome", Endereco = "Rua B" };
        Assert.IsType<OkObjectResult>(await c.UpdateEmpresa(request, 10));
        Assert.Equal(10, request.Id); Assert.Same(request, _sent);
    }

    [Fact]
    public async Task DashboardUsaNomeEEmpresaDoToken()
    {
        QueryResult(Result.Fail<DashboardResponseDto>(new NotFoundError("Sem dados")));
        var c = new DashboardController(NullLogger<DashboardController>.Instance, _mediator.Object) { ControllerContext = TestData.Context() };
        Assert.IsType<NotFoundObjectResult>(await c.GetResumoAsync());
        var query = Assert.IsType<GetDashboardResumoQuery>(_sent);
        Assert.Equal(10, query.EmpresaId); Assert.Equal("Ana", query.NomeUsuario);
    }

    [Fact]
    public async Task UserConsultaPerfilUsaIdentidadeDoToken()
    {
        var payload = new UsuarioResponse { Id = 7, Nome = "Ana", Email = "ana@example.com", Role = "Dono" };
        QueryResult(Result.Ok(payload));
        var c = new UserController(NullLogger<UserController>.Instance, _mediator.Object) { ControllerContext = TestData.Context() };
        Assert.Same(payload, Assert.IsType<OkObjectResult>(await c.GetUserById()).Value);
        var query = Assert.IsType<GetUsuarioQuery>(_sent);
        Assert.Equal(7, query.Id); Assert.Equal(10, query.EmpresaId);
    }

    [Fact]
    public async Task AuthRetornaTokensDoLoginERefresh()
    {
        var tokens = new TokensResponse { AccessToken = "access-test", RefreshToken = "refresh-test" };
        QueryResult(Result.Ok(tokens));
        var c = new AuthContoller(_mediator.Object, NullLogger<AuthContoller>.Instance) { ControllerContext = TestData.Context() };
        var login = new LoginCommand { Email = "ana@example.com", Senha = "senha123" };
        Assert.Same(tokens, Assert.IsType<OkObjectResult>(await c.Login(login)).Value); Assert.Same(login, _sent);
        var refresh = new RefreshTokenCommand { RefreshToken = "refresh-antigo" };
        Assert.Same(tokens, Assert.IsType<OkObjectResult>(await c.RefreshToken(refresh)).Value); Assert.Same(refresh, _sent);
    }
}
