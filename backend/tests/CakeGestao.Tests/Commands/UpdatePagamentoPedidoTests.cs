using CakeGestao.Application.Features.Pedidos.Command.UpdatePagemento;
using CakeGestao.Application.Features.Financeiro.Command.Cancelamento;
using CakeGestao.Application.Features.Financeiro.Command.Create;
using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Exceptions;
using CakeGestao.Domain.Interfaces.Repositories;
using CakeGestao.Infrastructure.Data;
using CakeGestao.Infrastructure.Data.Repositories;
using CakeGestao.Tests.Support;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CakeGestao.Tests.Commands;

public class UpdatePagamentoPedidoTests
{
    [Theory]
    [InlineData("ausente")]
    [InlineData("cancelada")]
    [InlineData("outro_pedido")]
    [InlineData("outra_empresa")]
    [InlineData("saida")]
    public async Task EstornoRecusaTransacaoIncorretaSemAlterarPedido(string scenario)
    {
        var pedido = PedidoPago();
        var pagamento = pedido.DataPagamento;
        var pedidos = Pedidos(pedido);
        var financeiro = new Mock<IFinanceiroRepository>();
        var transacao = new TransacaoFinanceira(scenario == "saida" ? TipoTransacaoEnum.Saida : TipoTransacaoEnum.Entrada,
            CategoriasEnum.Vendas, 90, DateTime.UtcNow, "Pagamento", scenario == "outro_pedido" ? 2 : 1,
            scenario == "outra_empresa" ? 20 : 10) { Id = 9 };
        if (scenario == "cancelada") transacao.Cancelar("Anterior", 7);
        financeiro.Setup(r => r.GetTransacaoByPedidoId(1, 10))
            .ReturnsAsync(scenario == "ausente" ? Result.Fail<TransacaoFinanceira>("Ausente") : Result.Ok(transacao));
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        var result = await Handler(pedidos.Object, financeiro.Object, mediator.Object).Handle(Request(false), default);
        Assert.IsType<ConflictError>(Assert.Single(result.Errors));
        Assert.True(pedido.Pago); Assert.Equal(pagamento, pedido.DataPagamento);
        pedidos.Verify(r => r.UpdatePedidoAsync(It.IsAny<Pedido>()), Times.Never);
        mediator.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task EstornoSoAlteraPedidoDepoisDoCancelamento(bool sucesso)
    {
        var pedido = PedidoPago(); var pagamento = pedido.DataPagamento;
        var pedidos = Pedidos(pedido);
        var financeiro = new Mock<IFinanceiroRepository>();
        financeiro.Setup(r => r.GetTransacaoByPedidoId(1, 10)).ReturnsAsync(Result.Ok(
            new TransacaoFinanceira(TipoTransacaoEnum.Entrada, CategoriasEnum.Vendas, 90, DateTime.UtcNow, "Pagamento", 1, 10) { Id = 9 }));
        using var cts = new CancellationTokenSource();
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        mediator.Setup(m => m.Send(It.Is<CancelTransacaoCommand>(c => c.TransacaoId == 9 && c.EmpresaId == 10 && c.UsuarioId == 7), cts.Token))
            .Callback(() => { Assert.True(pedido.Pago); Assert.Equal(pagamento, pedido.DataPagamento); })
            .ReturnsAsync(sucesso ? Result.Ok() : Result.Fail(new ValidationError("Cancelamento recusado")));
        var result = await Handler(pedidos.Object, financeiro.Object, mediator.Object).Handle(Request(false), cts.Token);
        Assert.Equal(sucesso, result.IsSuccess); Assert.Equal(!sucesso, pedido.Pago);
        if (sucesso) Assert.Null(pedido.DataPagamento);
        else { Assert.Equal(pagamento, pedido.DataPagamento); Assert.IsType<ValidationError>(Assert.Single(result.Errors)); }
        pedidos.Verify(r => r.UpdatePedidoAsync(pedido), sucesso ? Times.Once() : Times.Never());
        mediator.VerifyAll();
    }

    [Fact]
    public async Task PedidoJaNaoPagoNaoCancelaNovamente()
    {
        var pedido = TestData.Pedido();
        var financeiro = new Mock<IFinanceiroRepository>(MockBehavior.Strict);
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        Assert.True((await Handler(Pedidos(pedido).Object, financeiro.Object, mediator.Object).Handle(Request(false), default)).IsSuccess);
        financeiro.VerifyNoOtherCalls(); mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CicloPagarEstornarPagarEstornarCancelaCadaLancamentoCorreto()
    {
        var options = new DbContextOptionsBuilder<CageContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var db = new CageContext(options);
        var pedido = TestData.Pedido(); pedido.AdicionarItem(new(1, 3, 30));
        db.Receitas.Add(TestData.Receita()); db.Pedidos.Add(pedido); await db.SaveChangesAsync();
        var financeiro = new FinanceiroRepository(db, NullLogger<FinanceiroRepository>.Instance);
        var pedidos = new PedidoRepository(db, NullLogger<EmpresaRepository>.Instance);
        var empresas = TestData.Empresas().Object;
        var create = new CreateTransacaoHandler(financeiro, NullLogger<CreateTransacaoHandler>.Instance, new CreateTransacaoValidator(empresas));
        var cancel = new CancelTransacaoHandler(financeiro, new CancelTransacaoValidator(empresas), NullLogger<CancelTransacaoHandler>.Instance);
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        mediator.Setup(m => m.Send(It.IsAny<CreateTransacaoCommand>(), It.IsAny<CancellationToken>()))
            .Returns((CreateTransacaoCommand c, CancellationToken ct) => create.Handle(c, ct));
        mediator.Setup(m => m.Send(It.IsAny<CancelTransacaoCommand>(), It.IsAny<CancellationToken>()))
            .Returns((CancelTransacaoCommand c, CancellationToken ct) => cancel.Handle(c, ct));
        var handler = Handler(pedidos, financeiro, mediator.Object);
        Assert.True((await handler.Handle(Request(true), default)).IsSuccess);
        Assert.True((await handler.Handle(Request(false), default)).IsSuccess);
        var primeiro = await db.TransacoesFinanceiras.SingleAsync();
        Assert.True(primeiro.IsCancelado);
        var primeiroCancelamento = primeiro.DataCancelamento;
        Assert.True((await handler.Handle(Request(true), default)).IsSuccess);
        var segundo = await db.TransacoesFinanceiras.SingleAsync(t => !t.IsCancelado);
        Assert.NotEqual(primeiro.Id, segundo.Id);
        Assert.True((await handler.Handle(Request(false), default)).IsSuccess);
        db.ChangeTracker.Clear();
        var persisted = await db.Pedidos.FindAsync(1);
        Assert.False(persisted!.Pago); Assert.Null(persisted.DataPagamento);
        var transacoes = await db.TransacoesFinanceiras.OrderBy(t => t.Id).ToListAsync();
        Assert.Equal(2, transacoes.Count); Assert.All(transacoes, t => Assert.True(t.IsCancelado));
        Assert.Equal(primeiroCancelamento, transacoes[0].DataCancelamento);
        mediator.Verify(m => m.Send(It.Is<CancelTransacaoCommand>(c => c.TransacaoId == primeiro.Id), It.IsAny<CancellationToken>()), Times.Once);
        mediator.Verify(m => m.Send(It.Is<CancelTransacaoCommand>(c => c.TransacaoId == segundo.Id), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Pedido PedidoPago() { var p = TestData.Pedido(); p.AdicionarItem(new(1, 3, 30)); p.InformarPagamento(); return p; }
    private static UpdatePagamentoPedidoCommand Request(bool pagamento) => new() { PedidoId = 1, EmpresaId = 10, UsuarioId = 7, Pagamento = pagamento };
    private static Mock<IPedidoRepository> Pedidos(Pedido pedido)
    {
        var repo = new Mock<IPedidoRepository>(); repo.Setup(r => r.GetByIdAsync(1, 10)).ReturnsAsync(Result.Ok(pedido));
        repo.Setup(r => r.UpdatePedidoAsync(pedido)).ReturnsAsync(Result.Ok()); return repo;
    }
    private static UpdatePagamentoPedidoHandler Handler(IPedidoRepository pedidos, IFinanceiroRepository financeiro, IMediator mediator) =>
        new(pedidos, new UpdateSenhaValidator(TestData.Empresas().Object), mediator, NullLogger<UpdatePagamentoPedidoHandler>.Instance, financeiro);
}
