using CakeGestao.Application.Features.Pedidos.Command.Update;
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

public class UpdatePedidoTests
{
    [Theory]
    [InlineData("quantidade")]
    [InlineData("receita")]
    [InlineData("adicao")]
    [InlineData("remocao")]
    [InlineData("repetidos")]
    public async Task AlteracaoDeItensPagosCancelaAntesDeModificarPedido(string scenario)
    {
        var pedido = TestData.Pedido();
        pedido.AdicionarItem(new(1, 3, 30));
        pedido.AdicionarItem(new(2, 1, 30));
        pedido.InformarPagamento();
        var request = Request();
        request.Itens = scenario switch {
            "quantidade" => new() { new() { ReceitaId = 1, Quantidade = 2 }, new() { ReceitaId = 2, Quantidade = 1 } },
            "receita" => new() { new() { ReceitaId = 3, Quantidade = 3 }, new() { ReceitaId = 2, Quantidade = 1 } },
            "adicao" => new() { new() { ReceitaId = 1, Quantidade = 3 }, new() { ReceitaId = 2, Quantidade = 1 }, new() { ReceitaId = 3, Quantidade = 1 } },
            "remocao" => new() { new() { ReceitaId = 1, Quantidade = 3 } },
            _ => new() { new() { ReceitaId = 1, Quantidade = 3 }, new() { ReceitaId = 1, Quantidade = 3 } }
        };
        var pedidos = Pedidos(pedido);
        var receitas = Receitas();
        var financeiro = Financeiro();
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        mediator.Setup(m => m.Send(It.IsAny<CancelTransacaoCommand>(), It.IsAny<CancellationToken>()))
            .Callback(() => { Assert.True(pedido.Pago); Assert.Equal(120, pedido.ValorTotal); Assert.Equal("Cliente", pedido.ClienteNome); })
            .ReturnsAsync(Result.Ok());
        var result = await Handler(pedidos, receitas, financeiro, mediator).Handle(request, default);
        Assert.True(result.IsSuccess); Assert.False(pedido.Pago); Assert.Null(pedido.DataPagamento);
        Assert.Equal(request.Itens.Sum(i => i.Quantidade * 30m), pedido.ValorTotal);
        Assert.Equal(request.Itens.Select(i => (i.ReceitaId, i.Quantidade)), pedido.Itens.Select(i => (i.ReceitaId, i.Quantidade)));
        mediator.Verify(m => m.Send(It.Is<CancelTransacaoCommand>(c => c.TransacaoId == 9 && c.EmpresaId == 10 && c.UsuarioId == 7 && c.Motivo.Contains("Pedido #1")), It.IsAny<CancellationToken>()), Times.Once);
        pedidos.Verify(r => r.UpdatePedidoAsync(pedido), Times.Once);
    }

    [Fact]
    public async Task MesmosItensEmOutraOrdemPreservamPagamentoEPrecoOriginal()
    {
        var pedido = TestData.Pedido(); pedido.AdicionarItem(new(1, 3, 20)); pedido.AdicionarItem(new(2, 1, 10)); pedido.InformarPagamento();
        var pagamento = pedido.DataPagamento;
        var request = Request(); request.Itens = new() { new() { ReceitaId = 2, Quantidade = 1 }, new() { ReceitaId = 1, Quantidade = 3 } };
        var pedidos = Pedidos(pedido); var receitas = new Mock<IReceitaRepository>(MockBehavior.Strict);
        var financeiro = new Mock<IFinanceiroRepository>(MockBehavior.Strict); var mediator = new Mock<IMediator>(MockBehavior.Strict);
        Assert.True((await Handler(pedidos, receitas, financeiro, mediator).Handle(request, default)).IsSuccess);
        Assert.True(pedido.Pago); Assert.Equal(pagamento, pedido.DataPagamento); Assert.Equal(70, pedido.ValorTotal); Assert.Equal("Nome editado", pedido.ClienteNome);
        receitas.VerifyNoOtherCalls(); financeiro.VerifyNoOtherCalls(); mediator.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("cancelamento")]
    [InlineData("transacao_ausente")]
    [InlineData("receita_ausente")]
    public async Task FalhaPreservaPedidoENaoPersisteEdicao(string failure)
    {
        var pedido = TestData.Pedido(); pedido.AdicionarItem(new(1, 3, 30)); pedido.InformarPagamento();
        var pedidos = Pedidos(pedido); var receitas = Receitas(); var financeiro = Financeiro();
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        if (failure == "receita_ausente") receitas.Setup(r => r.GetReceitaByIdAsync(1, 10)).ReturnsAsync(Result.Fail<Receita>("Ausente"));
        if (failure == "transacao_ausente") financeiro.Setup(r => r.GetTransacaoByPedidoId(1, 10)).ReturnsAsync(Result.Fail<TransacaoFinanceira>("Ausente"));
        if (failure == "cancelamento") mediator.Setup(m => m.Send(It.IsAny<CancelTransacaoCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Fail(new ValidationError("Falha no cancelamento")));
        Assert.True((await Handler(pedidos, receitas, financeiro, mediator).Handle(Request(), default)).IsFailed);
        Assert.True(pedido.Pago); Assert.Equal(90, pedido.ValorTotal); Assert.Equal(3, Assert.Single(pedido.Itens).Quantidade); Assert.Equal("Cliente", pedido.ClienteNome);
        pedidos.Verify(r => r.UpdatePedidoAsync(It.IsAny<Pedido>()), Times.Never);
        if (failure != "cancelamento") mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task PedidoNaoPagoAlteraItensSemCancelarTransacao()
    {
        var pedido = TestData.Pedido(); pedido.AdicionarItem(new(1, 3, 30));
        var financeiro = new Mock<IFinanceiroRepository>(MockBehavior.Strict); var mediator = new Mock<IMediator>(MockBehavior.Strict);
        Assert.True((await Handler(Pedidos(pedido), Receitas(), financeiro, mediator).Handle(Request(), default)).IsSuccess);
        Assert.Equal(60, pedido.ValorTotal); Assert.False(pedido.Pago);
        financeiro.VerifyNoOtherCalls(); mediator.VerifyNoOtherCalls();
    }

    private static UpdatePedidoCommand Request() => new() { Id = 1, EmpresaId = 10, UsuarioId = 7, ClienteNome = "Nome editado", Itens = new() { new() { ReceitaId = 1, Quantidade = 2 } } };
    private static Mock<IPedidoRepository> Pedidos(Pedido pedido)
    {
        var repo = new Mock<IPedidoRepository>(); repo.Setup(r => r.GetByIdAsync(1, 10)).ReturnsAsync(Result.Ok(pedido));
        repo.Setup(r => r.UpdatePedidoAsync(pedido)).ReturnsAsync(Result.Ok()); return repo;
    }
    private static Mock<IReceitaRepository> Receitas()
    {
        var repo = new Mock<IReceitaRepository>(); repo.Setup(r => r.GetReceitaByIdAsync(It.IsAny<int>(), 10)).ReturnsAsync((int id, int empresa) => Result.Ok(TestData.Receita(id, empresa))); return repo;
    }
    private static Mock<IFinanceiroRepository> Financeiro()
    {
        var repo = new Mock<IFinanceiroRepository>(); repo.Setup(r => r.GetTransacaoByPedidoId(1, 10)).ReturnsAsync(Result.Ok(new TransacaoFinanceira(TipoTransacaoEnum.Entrada, CategoriasEnum.Vendas, 90, DateTime.UtcNow, "Pagamento", 1, 10) { Id = 9 })); return repo;
    }
    private static UpdatePedidoHandler Handler(Mock<IPedidoRepository> pedidos, Mock<IReceitaRepository> receitas, Mock<IFinanceiroRepository> financeiro, Mock<IMediator> mediator) =>
        new(new UpdatePedidoValidator(TestData.Empresas().Object), NullLogger<UpdatePedidoHandler>.Instance, pedidos.Object, receitas.Object, financeiro.Object, mediator.Object);
}
