using CakeGestao.Application.Features.Pedidos.Command.Update;
using CakeGestao.Application.Features.Pedidos.Command.UpdateStatus;
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

public class UpdatePedidoProducaoTests
{
    [Theory]
    [InlineData("adicao", false)]
    [InlineData("quantidade", false)]
    [InlineData("remocao", false)]
    [InlineData("troca", false)]
    [InlineData("adicao", true)]
    public async Task EdicaoDevolveListaAntigaEProximaProducaoDescontaListaNova(string scenario, bool pago)
    {
        var pedido = TestData.Pedido();
        pedido.AdicionarItem(new(1, 3, 30)); pedido.AdicionarItem(new(2, 2, 30));
        pedido.AlterarStatus(StatusPedidoEnum.EmProducao);
        if (pago) pedido.InformarPagamento();
        var pedidos = Pedidos(pedido);
        var receitas = Receitas();
        // Saldo após a baixa anterior: 3 * 0,2 + 2 * 0,1 = 0,8 kg.
        var estoque = TestData.Estoque(quantidade: 9.2m);
        var estoques = new Mock<IEstoqueRepository>(MockBehavior.Strict);
        estoques.Setup(r => r.GetItemEstoqueByIdAsync(1, 10)).ReturnsAsync(Result.Ok(estoque));
        estoques.Setup(r => r.UpdateItemEstoqueAsync(estoque)).Returns(Task.CompletedTask);
        var statusHandler = new UpdateStatusPedidoHandler(pedidos.Object, NullLogger<UpdateStatusPedidoHandler>.Instance,
            new UpdateStatusPedidoValidator(TestData.Empresas().Object), receitas.Object, estoques.Object);
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        mediator.Setup(m => m.Send(It.IsAny<UpdateStatusPedidoCommand>(), It.IsAny<CancellationToken>()))
            .Returns((UpdateStatusPedidoCommand c, CancellationToken ct) => statusHandler.Handle(c, ct));
        var financeiro = new Mock<IFinanceiroRepository>(MockBehavior.Strict);
        if (pago)
        {
            financeiro.Setup(r => r.GetTransacaoByPedidoId(1, 10)).ReturnsAsync(Result.Ok(new TransacaoFinanceira(
                TipoTransacaoEnum.Entrada, CategoriasEnum.Vendas, 150, DateTime.UtcNow, "Pagamento", 1, 10) { Id = 9 }));
            mediator.Setup(m => m.Send(It.Is<CancelTransacaoCommand>(c => c.TransacaoId == 9 && c.UsuarioId == 7), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Ok());
        }
        var request = Request();
        request.Itens = scenario switch {
            "adicao" => new() { new() { ReceitaId = 1, Quantidade = 3 }, new() { ReceitaId = 2, Quantidade = 2 }, new() { ReceitaId = 3, Quantidade = 1 } },
            "quantidade" => new() { new() { ReceitaId = 1, Quantidade = 2 }, new() { ReceitaId = 2, Quantidade = 2 } },
            "remocao" => new() { new() { ReceitaId = 1, Quantidade = 3 } },
            _ => new() { new() { ReceitaId = 3, Quantidade = 3 }, new() { ReceitaId = 2, Quantidade = 2 } }
        };
        var result = await EditHandler(pedidos, receitas, financeiro, mediator).Handle(request, default);
        Assert.True(result.IsSuccess);
        Assert.Equal(StatusPedidoEnum.Pendente, pedido.StatusPedido); Assert.False(pedido.Pago);
        Assert.Equal(10m, estoque.QuantidadeAtual); Assert.Equal(5m, estoque.ValorMedia);
        Assert.Equal(request.Itens.Select(i => (i.ReceitaId, i.Quantidade)), pedido.Itens.Select(i => (i.ReceitaId, i.Quantidade)));
        mediator.Verify(m => m.Send(It.Is<UpdateStatusPedidoCommand>(c => c.PedidoId == 1 && c.EmpresaId == 10 && c.Status == "Pendente"), It.IsAny<CancellationToken>()), Times.Once);
        // Salvar novamente a mesma lista não devolve outra vez.
        Assert.True((await EditHandler(pedidos, receitas, financeiro, mediator).Handle(request, default)).IsSuccess);
        Assert.Equal(10m, estoque.QuantidadeAtual);
        Assert.True((await statusHandler.Handle(new() { PedidoId = 1, EmpresaId = 10, Status = "EmProducao" }, default)).IsSuccess);
        var consumoNovo = request.Itens.Sum(i => i.Quantidade * Consumo(i.ReceitaId));
        Assert.Equal(10m - consumoNovo, estoque.QuantidadeAtual);
    }

    [Theory]
    [InlineData(StatusPedidoEnum.Pendente)]
    [InlineData(StatusPedidoEnum.ProntoEntregue)]
    [InlineData(StatusPedidoEnum.Concluido)]
    public async Task AlteracaoEmOutroStatusVoltaParaPendenteSemDevolver(StatusPedidoEnum status)
    {
        var pedido = TestData.Pedido(); pedido.AdicionarItem(new(1, 3, 30)); pedido.AlterarStatus(status);
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        var result = await EditHandler(Pedidos(pedido), Receitas(), new(MockBehavior.Strict), mediator).Handle(Request(), default);
        Assert.True(result.IsSuccess); Assert.Equal(StatusPedidoEnum.Pendente, pedido.StatusPedido);
        mediator.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(StatusPedidoEnum.EmProducao)]
    [InlineData(StatusPedidoEnum.Concluido)]
    public async Task AlterarDadosOuOrdemSemMudarItensPreservaStatus(StatusPedidoEnum status)
    {
        var pedido = TestData.Pedido(); pedido.AdicionarItem(new(1, 3, 30)); pedido.AdicionarItem(new(2, 2, 30)); pedido.AlterarStatus(status);
        var request = Request(); request.Itens = new() { new() { ReceitaId = 2, Quantidade = 2 }, new() { ReceitaId = 1, Quantidade = 3 } };
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        var receitas = new Mock<IReceitaRepository>(MockBehavior.Strict);
        Assert.True((await EditHandler(Pedidos(pedido), receitas, new(MockBehavior.Strict), mediator).Handle(request, default)).IsSuccess);
        Assert.Equal(status, pedido.StatusPedido); Assert.Equal("Novo nome", pedido.ClienteNome);
        mediator.VerifyNoOtherCalls(); receitas.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task FalhaNaDevolucaoNaoSubstituiItensNemSalvaEdicao()
    {
        var pedido = TestData.Pedido(); pedido.AdicionarItem(new(1, 3, 30)); pedido.AlterarStatus(StatusPedidoEnum.EmProducao);
        var pedidos = Pedidos(pedido);
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        mediator.Setup(m => m.Send(It.IsAny<UpdateStatusPedidoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail(new NotFoundError("Item não encontrado para devolução")));
        var result = await EditHandler(pedidos, Receitas(), new(MockBehavior.Strict), mediator).Handle(Request(), default);
        Assert.IsType<NotFoundError>(Assert.Single(result.Errors));
        Assert.Equal(StatusPedidoEnum.EmProducao, pedido.StatusPedido); Assert.Equal(3, Assert.Single(pedido.Itens).Quantidade);
        Assert.Equal("Cliente", pedido.ClienteNome);
        pedidos.Verify(r => r.UpdatePedidoAsync(It.IsAny<Pedido>()), Times.Never);
    }

    private static decimal Consumo(int id) => id == 1 ? 0.2m : id == 2 ? 0.1m : 0.4m;
    private static UpdatePedidoCommand Request() => new() { Id = 1, EmpresaId = 10, UsuarioId = 7, ClienteNome = "Novo nome", Itens = new() { new() { ReceitaId = 1, Quantidade = 2 } } };
    private static Mock<IPedidoRepository> Pedidos(Pedido pedido)
    {
        var repo = new Mock<IPedidoRepository>(); repo.Setup(r => r.GetByIdAsync(1, 10)).ReturnsAsync(Result.Ok(pedido));
        repo.Setup(r => r.UpdatePedidoAsync(pedido)).ReturnsAsync(Result.Ok()); return repo;
    }
    private static Mock<IReceitaRepository> Receitas()
    {
        var repo = new Mock<IReceitaRepository>();
        repo.Setup(r => r.GetReceitaByIdAsync(It.IsAny<int>(), 10)).ReturnsAsync((int id, int empresa) => {
            var receita = TestData.Receita(id, empresa); receita.AdicionarIngrediente(new(1, Consumo(id), UnidadeMedidaEnum.KG));
            return Result.Ok(receita);
        }); return repo;
    }
    private static UpdatePedidoHandler EditHandler(Mock<IPedidoRepository> pedidos, Mock<IReceitaRepository> receitas, Mock<IFinanceiroRepository> financeiro, Mock<IMediator> mediator) =>
        new(new UpdatePedidoValidator(TestData.Empresas().Object), NullLogger<UpdatePedidoHandler>.Instance, pedidos.Object, receitas.Object, financeiro.Object, mediator.Object);
}
