using CakeGestao.Application.Features.Pedidos.Command.UpdateStatus;
using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Exceptions;
using CakeGestao.Domain.Interfaces.Repositories;
using CakeGestao.Tests.Support;
using FluentResults;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CakeGestao.Tests.Commands;

public class UpdateStatusPedidoTests
{
    public static IEnumerable<object[]> Transicoes()
    {
        foreach (var origem in Enum.GetValues<StatusPedidoEnum>())
        foreach (var destino in Enum.GetValues<StatusPedidoEnum>())
            yield return new object[] { origem, destino };
    }

    [Theory]
    [MemberData(nameof(Transicoes))]
    public async Task MovimentaEstoqueSomenteNasTransicoesDefinidas(StatusPedidoEnum origem, StatusPedidoEnum destino)
    {
        var pedido = TestData.Pedido();
        pedido.AdicionarItem(new(1, 3, 30)); pedido.AlterarStatus(origem);
        var receita = TestData.Receita(); receita.AdicionarIngrediente(new(1, 0.2m, UnidadeMedidaEnum.KG));
        var estoque = TestData.Estoque(quantidade: 2);
        var pedidos = Pedidos(pedido);
        var receitas = new Mock<IReceitaRepository>(MockBehavior.Strict);
        var estoques = new Mock<IEstoqueRepository>(MockBehavior.Strict);
        bool baixa = origem == StatusPedidoEnum.Pendente && destino == StatusPedidoEnum.EmProducao;
        bool devolucao = origem == StatusPedidoEnum.EmProducao && destino == StatusPedidoEnum.Pendente;
        if (baixa || devolucao)
        {
            receitas.Setup(r => r.GetReceitaByIdAsync(1, 10)).ReturnsAsync(Result.Ok(receita));
            estoques.Setup(r => r.GetItemEstoqueByIdAsync(1, 10)).ReturnsAsync(Result.Ok(estoque));
            estoques.Setup(r => r.UpdateItemEstoqueAsync(estoque)).Returns(Task.CompletedTask);
        }
        var result = await Handler(pedidos, receitas, estoques).Handle(Request(destino), default);
        Assert.True(result.IsSuccess); Assert.Equal(destino, pedido.StatusPedido);
        Assert.Equal(baixa ? 1.4m : devolucao ? 2.6m : 2m, estoque.QuantidadeAtual);
        Assert.Equal(5m, estoque.ValorMedia);
        estoques.Verify(r => r.UpdateItemEstoqueAsync(It.IsAny<Estoque>()), Times.Never());
        if (!baixa && !devolucao) { receitas.VerifyNoOtherCalls(); estoques.VerifyNoOtherCalls(); }
    }

    [Theory]
    [InlineData(2)]
    [InlineData(-1)]
    public async Task CicloDeProducaoDevolveIngredientesCompartilhadosSemDuplicar(int saldoInicial)
    {
        var pedido = TestData.Pedido(); pedido.AdicionarItem(new(1, 3, 30)); pedido.AdicionarItem(new(2, 2, 30));
        var receita1 = TestData.Receita(1); receita1.AdicionarIngrediente(new(1, 0.2m, UnidadeMedidaEnum.KG));
        var receita2 = TestData.Receita(2); receita2.AdicionarIngrediente(new(1, 0.1m, UnidadeMedidaEnum.KG));
        var estoque = TestData.Estoque(quantidade: saldoInicial);
        var receitas = new Mock<IReceitaRepository>();
        receitas.Setup(r => r.GetReceitaByIdAsync(1, 10)).ReturnsAsync(Result.Ok(receita1));
        receitas.Setup(r => r.GetReceitaByIdAsync(2, 10)).ReturnsAsync(Result.Ok(receita2));
        var estoques = new Mock<IEstoqueRepository>();
        estoques.Setup(r => r.GetItemEstoqueByIdAsync(1, 10)).ReturnsAsync(Result.Ok(estoque));
        var handler = Handler(Pedidos(pedido), receitas, estoques);
        Assert.True((await handler.Handle(Request(StatusPedidoEnum.EmProducao), default)).IsSuccess);
        Assert.Equal(saldoInicial - 0.8m, estoque.QuantidadeAtual);
        var retorno = Request(StatusPedidoEnum.Pendente); retorno.Status = "pendente";
        Assert.True((await handler.Handle(retorno, default)).IsSuccess);
        Assert.Equal(saldoInicial, estoque.QuantidadeAtual); Assert.Equal(5, estoque.ValorMedia);
        Assert.True((await handler.Handle(retorno, default)).IsSuccess);
        Assert.Equal(saldoInicial, estoque.QuantidadeAtual);
        Assert.True((await handler.Handle(Request(StatusPedidoEnum.EmProducao), default)).IsSuccess);
        Assert.Equal(saldoInicial - 0.8m, estoque.QuantidadeAtual);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task FaltaDeReceitaOuEstoqueInterrompeDevolucaoSemMudarStatus(bool faltaReceita)
    {
        var pedido = TestData.Pedido(); pedido.AdicionarItem(new(1, 1, 30)); pedido.AlterarStatus(StatusPedidoEnum.EmProducao);
        var receita = TestData.Receita(); receita.AdicionarIngrediente(new(1, 0.2m, UnidadeMedidaEnum.KG)); receita.AdicionarIngrediente(new(2, 0.1m, UnidadeMedidaEnum.KG));
        var estoque = TestData.Estoque();
        var receitas = new Mock<IReceitaRepository>();
        receitas.Setup(r => r.GetReceitaByIdAsync(1, 10)).ReturnsAsync(faltaReceita ? Result.Fail<Receita>("Ausente") : Result.Ok(receita));
        var estoques = new Mock<IEstoqueRepository>();
        estoques.Setup(r => r.GetItemEstoqueByIdAsync(1, 10)).ReturnsAsync(Result.Ok(estoque));
        estoques.Setup(r => r.GetItemEstoqueByIdAsync(2, 10)).ReturnsAsync(Result.Fail<Estoque>("Ausente"));
        var pedidos = Pedidos(pedido);
        var result = await Handler(pedidos, receitas, estoques).Handle(Request(StatusPedidoEnum.Pendente), default);
        Assert.IsType<NotFoundError>(Assert.Single(result.Errors));
        Assert.Equal(StatusPedidoEnum.EmProducao, pedido.StatusPedido); Assert.Equal(10, estoque.QuantidadeAtual);
        pedidos.Verify(r => r.UpdatePedidoAsync(It.IsAny<Pedido>()), Times.Never);
        estoques.Verify(r => r.UpdateItemEstoqueAsync(It.IsAny<Estoque>()), Times.Never);
    }

    private static UpdateStatusPedidoCommand Request(StatusPedidoEnum status) => new() { PedidoId = 1, EmpresaId = 10, Status = status.ToString() };
    private static Mock<IPedidoRepository> Pedidos(Pedido pedido)
    {
        var repo = new Mock<IPedidoRepository>(); repo.Setup(r => r.GetByIdAsync(1, 10)).ReturnsAsync(Result.Ok(pedido));
        repo.Setup(r => r.UpdatePedidoAsync(pedido)).ReturnsAsync(Result.Ok()); return repo;
    }
    private static UpdateStatusPedidoHandler Handler(Mock<IPedidoRepository> pedidos, Mock<IReceitaRepository> receitas, Mock<IEstoqueRepository> estoques) =>
        new(pedidos.Object, NullLogger<UpdateStatusPedidoHandler>.Instance, new UpdateStatusPedidoValidator(TestData.Empresas().Object), receitas.Object, estoques.Object);
}
