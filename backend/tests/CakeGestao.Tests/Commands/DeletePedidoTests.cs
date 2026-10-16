using CakeGestao.Application.Features.Pedidos.Command.Delete;
using CakeGestao.Application.Features.Financeiro.Command.Cancelamento;
using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Exceptions;
using CakeGestao.Domain.Interfaces.Repositories;
using CakeGestao.Tests.Support;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CakeGestao.Tests.Commands;

public class DeletePedidoTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task CancelaTodosLancamentosAtivosAntesDeExcluir(int quantidade)
    {
        var pedido = TestData.Pedido();
        var pedidos = new Mock<IPedidoRepository>(MockBehavior.Strict);
        pedidos.Setup(r => r.GetByIdAsync(1, 10)).ReturnsAsync(Result.Ok(pedido));
        var transacoes = Enumerable.Range(1, quantidade).Select(id => new TransacaoFinanceira(
            TipoTransacaoEnum.Entrada, CategoriasEnum.Vendas, 100, DateTime.UtcNow, "Pagamento", 1, 10) { Id = id }).ToList();
        var financeiro = new Mock<IFinanceiroRepository>(MockBehavior.Strict);
        financeiro.Setup(r => r.GetTransacoesAtivasByPedidoIdAsync(1, 10)).ReturnsAsync(Result.Ok(transacoes));
        var cancelados = new List<int>();
        using var cts = new CancellationTokenSource();
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        mediator.Setup(m => m.Send(It.Is<CancelTransacaoCommand>(c => c.EmpresaId == 10 && c.UsuarioId == 7 && c.Motivo.Contains("Pedido #1")), cts.Token))
            .Callback<IRequest<Result>, CancellationToken>((c, _) => cancelados.Add(((CancelTransacaoCommand)c).TransacaoId)).ReturnsAsync(Result.Ok());
        pedidos.Setup(r => r.DeletePedidoAsync(pedido)).Callback(() => Assert.Equal(transacoes.Select(t => t.Id), cancelados)).ReturnsAsync(Result.Ok());
        var result = await Handler(pedidos, financeiro, mediator).Handle(Request(), cts.Token);
        Assert.True(result.IsSuccess);
        pedidos.Verify(r => r.DeletePedidoAsync(pedido), Times.Once);
        mediator.Verify(m => m.Send(It.IsAny<CancelTransacaoCommand>(), cts.Token), Times.Exactly(quantidade));
    }

    [Theory]
    [InlineData("consulta")]
    [InlineData("cancelamento")]
    [InlineData("exclusao")]
    public async Task PropagaFalhasESoExcluiAposCancelamento(string falha)
    {
        var pedido = TestData.Pedido();
        var pedidos = new Mock<IPedidoRepository>(); pedidos.Setup(r => r.GetByIdAsync(1, 10)).ReturnsAsync(Result.Ok(pedido));
        pedidos.Setup(r => r.DeletePedidoAsync(pedido)).ReturnsAsync(Result.Fail(new ConflictError("Falha ao excluir")));
        var financeiro = new Mock<IFinanceiroRepository>();
        financeiro.Setup(r => r.GetTransacoesAtivasByPedidoIdAsync(1, 10)).ReturnsAsync(falha == "consulta"
            ? Result.Fail<List<TransacaoFinanceira>>(new ConflictError("Falha na consulta"))
            : Result.Ok(new List<TransacaoFinanceira> { TestData.Transacao() }));
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<CancelTransacaoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(falha == "cancelamento" ? Result.Fail(new ConflictError("Falha ao cancelar")) : Result.Ok());
        var result = await Handler(pedidos, financeiro, mediator).Handle(Request(), default);
        Assert.IsType<ConflictError>(Assert.Single(result.Errors));
        pedidos.Verify(r => r.DeletePedidoAsync(It.IsAny<Pedido>()), falha == "exclusao" ? Times.Once() : Times.Never());
        mediator.Verify(m => m.Send(It.IsAny<CancelTransacaoCommand>(), It.IsAny<CancellationToken>()), falha == "consulta" ? Times.Never() : Times.Once());
    }

    [Fact]
    public async Task PedidoAusenteNaoCancelaNemExclui()
    {
        var pedidos = new Mock<IPedidoRepository>(MockBehavior.Strict);
        pedidos.Setup(r => r.GetByIdAsync(1, 10)).ReturnsAsync(Result.Fail<Pedido>("Ausente"));
        var financeiro = new Mock<IFinanceiroRepository>(MockBehavior.Strict);
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        var result = await Handler(pedidos, financeiro, mediator).Handle(Request(), default);
        Assert.IsType<NotFoundError>(Assert.Single(result.Errors));
        financeiro.VerifyNoOtherCalls(); mediator.VerifyNoOtherCalls();
    }
    private static DeletePedidoCommand Request() => new() { PedidoId = 1, EmpresaId = 10, UsuarioId = 7 };
    private static DeletePedidoHandler Handler(Mock<IPedidoRepository> pedidos, Mock<IFinanceiroRepository> financeiro, Mock<IMediator> mediator) =>
        new(pedidos.Object, new DeletePedidoValidator(TestData.Empresas().Object), NullLogger<DeletePedidoHandler>.Instance, financeiro.Object, mediator.Object);
}
