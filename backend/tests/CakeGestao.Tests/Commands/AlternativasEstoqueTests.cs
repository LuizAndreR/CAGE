using CakeGestao.Application.Common;
using CakeGestao.Application.Features.Pedidos.Command.UpdateStatus;
using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Interfaces.Repositories;
using CakeGestao.Tests.Support;
using FluentResults;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CakeGestao.Tests.Commands;

public class AlternativasEstoqueTests
{
    [Theory]
    [InlineData(2, 2, 1, 2, 0)] // principal suficiente
    [InlineData(0.3, 2, 0, 1.3, 0)] // completa na alternativa
    [InlineData(0, 2, 0, 1, 0)]
    [InlineData(0.3, 0.2, -0.5, 0, 1)] // restante negativo na principal
    [InlineData(-0.4, 2, -0.4, 1, 0)] // saldo negativo não está disponível
    [InlineData(-0.4, -0.2, -1.4, -0.2, 1)]
    public async Task ConsomeDisponivelERestanteFicaNaPrincipal(decimal saldoA, decimal saldoB,
        decimal finalA, decimal finalB, int avisos)
    {
        var a = TestData.Estoque(quantidade: saldoA);
        var b = TestData.Estoque(2, quantidade: saldoB);
        var (handler, pedido, _) = Preparar(new[] { a, b }, Ingrediente(1, 1, 2));
        var result = await handler.Handle(Status("EmProducao"), default);
        Assert.True(result.IsSuccess);
        Assert.Equal(finalA, a.QuantidadeAtual);
        Assert.Equal(finalB, b.QuantidadeAtual);
        Assert.Equal(avisos, result.Successes.OfType<EstoqueNegativoAviso>().Count());
        Assert.All(result.Successes.OfType<EstoqueNegativoAviso>(), aviso => {
            Assert.Equal(a.Id, aviso.ItemId); Assert.Equal(finalA, aviso.Saldo);
        });
        // Repetir o mesmo status não desconta novamente.
        await handler.Handle(Status("EmProducao"), default);
        Assert.Equal(finalA, a.QuantidadeAtual);
        Assert.Equal(finalB, b.QuantidadeAtual);
        await handler.Handle(Status("Pendente"), default);
        Assert.Equal(saldoA, a.QuantidadeAtual);
        Assert.Equal(saldoB, b.QuantidadeAtual);
        Assert.Empty(pedido.ConsumosEstoque);
    }

    [Fact]
    public async Task RespeitaOrdemDeTresMarcasEQuantidadeDoPedido()
    {
        var itens = new[] { TestData.Estoque(1, quantidade: 0.2m), TestData.Estoque(2, quantidade: 2), TestData.Estoque(3, quantidade: 0.3m) };
        var (handler, pedido, _) = Preparar(itens, Ingrediente(1, 0.5m, 3, 2));
        pedido.Itens.Clear(); pedido.AdicionarItem(new(1, 3, 30));
        Assert.True((await handler.Handle(Status("EmProducao"), default)).IsSuccess);
        Assert.Equal(0, itens[0].QuantidadeAtual);
        Assert.Equal(1, itens[1].QuantidadeAtual);
        Assert.Equal(0, itens[2].QuantidadeAtual);
        Assert.Equal(1.5m, pedido.ConsumosEstoque.Sum(c => c.Quantidade));
    }

    [Fact]
    public async Task ConverteKgParaGramasEDevolveSemDependerDaReceitaAtual()
    {
        var a = TestData.Estoque(quantidade: 0.3m);
        var b = new Estoque("Farinha", "B", 1000, UnidadeMedidaEnum.G, 0, 0.01m, 10, null, null) { Id = 2 };
        var (handler, pedido, receitas) = Preparar(new[] { a, b }, Ingrediente(1, 1, 2));
        Assert.True((await handler.Handle(Status("EmProducao"), default)).IsSuccess);
        Assert.Equal(300, b.QuantidadeAtual);
        Assert.Equal(700, pedido.ConsumosEstoque.Single(c => c.ItemId == 2).Quantidade);
        // Receita removida ou editada depois da produção: a devolução usa o registro do pedido.
        receitas.Setup(r => r.GetReceitaByIdAsync(1, 10)).ReturnsAsync(Result.Fail<Receita>("Alterada"));
        Assert.True((await handler.Handle(Status("Pendente"), default)).IsSuccess);
        Assert.Equal(0.3m, a.QuantidadeAtual); Assert.Equal(1000, b.QuantidadeAtual);
    }

    [Fact]
    public async Task NaoReutilizaSaldoCompartilhadoEntreIngredientesEReceitas()
    {
        var itens = new[] { TestData.Estoque(1, quantidade: 0), TestData.Estoque(2, quantidade: 1), TestData.Estoque(3, quantidade: 0) };
        var (handler, pedido, receitas) = Preparar(itens, Ingrediente(1, 0.8m, 2));
        var outra = TestData.Receita(2); outra.AdicionarIngrediente(Ingrediente(3, 0.8m, 2));
        receitas.Setup(r => r.GetReceitaByIdAsync(2, 10)).ReturnsAsync(Result.Ok(outra));
        pedido.AdicionarItem(new(2, 1, 30));
        var result = await handler.Handle(Status("EmProducao"), default);
        Assert.True(result.IsSuccess);
        Assert.Equal(0, itens[1].QuantidadeAtual);
        Assert.Equal(-0.6m, itens[2].QuantidadeAtual);
        Assert.Equal(1, pedido.ConsumosEstoque.Single(c => c.ItemId == 2).Quantidade);
    }

    [Fact]
    public async Task FalhaNoUltimoIngredienteNaoDescontaOsAnteriores()
    {
        var a = TestData.Estoque(quantidade: 1);
        var (handler, pedido, _) = Preparar(new[] { a }, Ingrediente(1, 0.5m), Ingrediente(99, 1));
        var result = await handler.Handle(Status("EmProducao"), default);
        Assert.True(result.IsFailed); Assert.Equal(1, a.QuantidadeAtual);
        Assert.Equal(StatusPedidoEnum.Pendente, pedido.StatusPedido); Assert.Empty(pedido.ConsumosEstoque);
    }

    [Fact]
    public async Task UnidadesIncompativeisNaoAlteramSaldo()
    {
        var a = TestData.Estoque(quantidade: 0.3m);
        var b = new Estoque("Farinha", "B", 1, UnidadeMedidaEnum.UN, 0, 1, 10, null, null) { Id = 2 };
        var (handler, pedido, _) = Preparar(new[] { a, b }, Ingrediente(1, 1, 2));
        Assert.True((await handler.Handle(Status("EmProducao"), default)).IsFailed);
        Assert.Equal(0.3m, a.QuantidadeAtual); Assert.Empty(pedido.ConsumosEstoque);
    }

    [Theory]
    [InlineData("principal")]
    [InlineData("repetida")]
    [InlineData("outra-empresa")]
    [InlineData("incompativel")]
    [InlineData("nula")]
    public async Task ValidaAlternativasNoCadastro(string caso)
    {
        var repo = new Mock<IEstoqueRepository>();
        repo.Setup(r => r.GetItemEstoqueByIdAsync(2, 10)).ReturnsAsync(caso == "outra-empresa"
            ? Result.Fail<Estoque>("Não encontrado")
            : Result.Ok(new Estoque("Farinha", "B", 1, UnidadeMedidaEnum.UN, 0, 1, 10, null, null) { Id = 2 }));
        List<int>? ids = caso switch { "principal" => new() { 1 }, "repetida" => new() { 2, 2 }, "nula" => null, _ => new() { 2 } };
        Assert.True((await AlternativasIngrediente.ConfigurarAsync(Ingrediente(1, 1), ids, 10, repo.Object)).IsFailed);
    }

    [Fact]
    public async Task AvisaTodosOsIngredientesQueFicaramNegativos()
    {
        var itens = new[] { TestData.Estoque(1, quantidade: 0.2m), TestData.Estoque(2, quantidade: 0) };
        var (handler, pedido, _) = Preparar(itens, Ingrediente(1, 1), Ingrediente(2, 0.5m));
        var result = await handler.Handle(Status("EmProducao"), default);
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Successes.OfType<EstoqueNegativoAviso>().Count());
        Assert.Equal(-0.8m, itens[0].QuantidadeAtual);
        Assert.Equal(-0.5m, itens[1].QuantidadeAtual);
    }

    public static IEnumerable<object[]> CompatibilidadeUnidades()
    {
        var grupos = new Dictionary<UnidadeMedidaEnum, string> {
            [UnidadeMedidaEnum.G] = "massa", [UnidadeMedidaEnum.KG] = "massa",
            [UnidadeMedidaEnum.ML] = "volume", [UnidadeMedidaEnum.L] = "volume",
            [UnidadeMedidaEnum.CUP] = "culinaria", [UnidadeMedidaEnum.TSP] = "culinaria",
            [UnidadeMedidaEnum.TBS] = "culinaria", [UnidadeMedidaEnum.UN] = "unidade"
        };
        foreach (var principal in grupos)
        foreach (var alternativa in grupos)
        {
            var esperado = principal.Value == alternativa.Value ||
                (principal.Value != "unidade" && alternativa.Value != "unidade" &&
                 (principal.Value == "culinaria" || alternativa.Value == "culinaria"));
            yield return new object[] { principal.Key, alternativa.Key, esperado };
        }
    }

    [Theory]
    [MemberData(nameof(CompatibilidadeUnidades))]
    public async Task CadastroRespeitaTiposMesmoComReferenciaDePeso(UnidadeMedidaEnum principal,
        UnidadeMedidaEnum alternativa, bool permitido)
    {
        var repo = new Mock<IEstoqueRepository>();
        var item = new Estoque("Alternativa", "B", 10, alternativa, 0, 1, 10, UnidadeMedidaEnum.CUP, 120) { Id = 2 };
        repo.Setup(r => r.GetItemEstoqueByIdAsync(2, 10)).ReturnsAsync(Result.Ok(item));
        var ingrediente = new Ingrediente(1, 1, principal);
        var result = await AlternativasIngrediente.ConfigurarAsync(ingrediente, new() { 2 }, 10, repo.Object);
        Assert.Equal(permitido, result.IsSuccess);
        Assert.Equal(permitido ? 1 : 0, ingrediente.Alternativas.Count);
    }

    [Fact]
    public async Task ProducaoRecusaAlternativaQuePassouDeMassaParaVolume()
    {
        var a = TestData.Estoque(quantidade: 0.3m);
        var b = new Estoque("Leite", "B", 1, UnidadeMedidaEnum.L, 0, 1, 10, UnidadeMedidaEnum.CUP, 120) { Id = 2 };
        var (handler, pedido, _) = Preparar(new[] { a, b }, Ingrediente(1, 1, 2));
        Assert.True((await handler.Handle(Status("EmProducao"), default)).IsFailed);
        Assert.Equal(0.3m, a.QuantidadeAtual);
        Assert.Equal(1, b.QuantidadeAtual);
        Assert.Empty(pedido.ConsumosEstoque);
    }

    private static Ingrediente Ingrediente(int id, decimal quantidade, params int[] alternativas)
    {
        var ingrediente = new Ingrediente(id, quantidade, UnidadeMedidaEnum.KG);
        for (var i = 0; i < alternativas.Length; i++) ingrediente.Alternativas.Add(new(alternativas[i], i));
        return ingrediente;
    }
    private static UpdateStatusPedidoCommand Status(string status) => new() { PedidoId = 1, EmpresaId = 10, Status = status };
    private static (UpdateStatusPedidoHandler, Pedido, Mock<IReceitaRepository>) Preparar(Estoque[] itens, params Ingrediente[] ingredientes)
    {
        var pedido = TestData.Pedido(); pedido.AdicionarItem(new(1, 1, 30));
        var receita = TestData.Receita(); foreach (var ing in ingredientes) receita.AdicionarIngrediente(ing);
        var pedidos = new Mock<IPedidoRepository>();
        pedidos.Setup(r => r.GetByIdAsync(1, 10)).ReturnsAsync(Result.Ok(pedido));
        pedidos.Setup(r => r.UpdatePedidoAsync(pedido)).ReturnsAsync(Result.Ok());
        var receitas = new Mock<IReceitaRepository>();
        receitas.Setup(r => r.GetReceitaByIdAsync(1, 10)).ReturnsAsync(Result.Ok(receita));
        var estoques = new Mock<IEstoqueRepository>();
        estoques.Setup(r => r.GetItemEstoqueByIdAsync(It.IsAny<int>(), 10)).ReturnsAsync((int id, int? empresa) =>
            itens.FirstOrDefault(i => i.Id == id) is { } item ? Result.Ok(item) : Result.Fail<Estoque>("Não encontrado"));
        return (new(pedidos.Object, NullLogger<UpdateStatusPedidoHandler>.Instance,
            new UpdateStatusPedidoValidator(TestData.Empresas().Object), receitas.Object, estoques.Object), pedido, receitas);
    }
}
