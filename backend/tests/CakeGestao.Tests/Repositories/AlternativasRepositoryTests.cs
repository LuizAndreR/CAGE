using CakeGestao.Application.Features.Pedidos.Command.UpdateStatus;
using CakeGestao.Application.Features.Receitas.Command.Create;
using CakeGestao.Application.Features.Receitas.Command.Update;
using CakeGestao.Application.Features.Receitas.Common;
using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Enum;
using CakeGestao.Infrastructure.Data;
using CakeGestao.Infrastructure.Data.Repositories;
using CakeGestao.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace CakeGestao.Tests.Repositories;

// Integração do fluxo com EF InMemory; não substitui a execução de migrations no PostgreSQL.
public class AlternativasRepositoryTests
{
    [Fact]
    public async Task CadastroEdicaoEConsultaMantemOrdemECustoApenasDoPrincipal()
    {
        var options = new DbContextOptionsBuilder<CageContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using var db = new CageContext(options);
        db.Estoque.AddRange(TestData.Estoque(),
            new Estoque("Farinha", "B", 10, UnidadeMedidaEnum.G, 0, 90, 10, null, null) { Id = 2 },
            TestData.Estoque(3));
        await db.SaveChangesAsync();
        var repo = new ReceitaRepository(db, NullLogger<ReceitaRepository>.Instance);
        var estoque = new EstoqueRepository(db, NullLogger<EstoqueRepository>.Instance);
        var create = new CreateReceitaHandler(repo, new CreateReceitaValidator(TestData.Empresas().Object), estoque, NullLogger<CreateReceitaHandler>.Instance);
        Assert.True((await create.Handle(new() {
            EmpresaId = 10, Nome = "Bolo", ModoPreparo = "Assar", PercentualMargemLucro = 100,
            Ingredientes = new() { new() { ItemId = 1, Quantidade = 1, UnidadeMedida = "KG", AlternativasIds = new() { 2, 3 } } }
        }, default)).IsSuccess);
        db.ChangeTracker.Clear();
        var receita = (await repo.GetReceitaByIdAsync(1, 10)).Value;
        Assert.Equal(5, receita.PrecoIngredientes);
        var dto = TestData.Mapper().Map<ReceitaResponse>(receita);
        Assert.Equal(new[] { 2, 3 }, dto.Ingredientes[0].AlternativasIds);
        Assert.Equal("Marca A", dto.Ingredientes[0].Marca);
        Assert.True(await estoque.ExisteDependenciaComReceitaAsync(2, 10));
        Assert.False(await estoque.ExisteDependenciaComReceitaAsync(2, 20));
        var update = new UpdateReceitaHandler(repo, estoque, NullLogger<UpdateReceitaHandler>.Instance,
            new UpdateReceitaValidator(TestData.Empresas().Object));
        var request = new UpdateReceitaCommand { Id = 1, EmpresaId = 10, Nome = "Bolo", ModoPreparo = "Assar",
            Ingredientes = new() { new() { ItemId = 1, Quantidade = 1, UnidadeMedida = "KG", AlternativasIds = new() { 3, 2 } } } };
        Assert.True((await update.Handle(request, default)).IsSuccess);
        db.ChangeTracker.Clear();
        receita = (await repo.GetReceitaByIdAsync(1, 10)).Value;
        Assert.Equal(new[] { 3, 2 }, receita.Ingredientes.Single().Alternativas.OrderBy(a => a.Ordem).Select(a => a.ItemId));
        Assert.Equal(2, await db.Set<IngredienteAlternativa>().CountAsync());
        // Salvar novamente sem mudar ingredientes não pode zerar o custo estimado.
        Assert.True((await update.Handle(request, default)).IsSuccess);
        Assert.Equal(5, receita.CustoTotal);
        request.Ingredientes[0].AlternativasIds.Clear();
        Assert.True((await update.Handle(request, default)).IsSuccess);
        db.ChangeTracker.Clear();
        Assert.Empty((await repo.GetReceitaByIdAsync(1, 10)).Value.Ingredientes.Single().Alternativas);
        Assert.Empty(await db.Set<IngredienteAlternativa>().ToListAsync());
    }

    [Fact]
    public async Task ProducaoPersisteEstoqueEConsumoJuntosEDevolucaoRemoveRegistro()
    {
        var options = new DbContextOptionsBuilder<CageContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using var db = new CageContext(options);
        db.Estoque.AddRange(TestData.Estoque(quantidade: 0.3m), TestData.Estoque(2, quantidade: 2));
        var ingrediente = new Ingrediente(1, 1, UnidadeMedidaEnum.KG); ingrediente.Alternativas.Add(new(2, 0));
        var receita = TestData.Receita(); receita.AdicionarIngrediente(ingrediente); db.Receitas.Add(receita);
        var pedido = TestData.Pedido(); pedido.AdicionarItem(new(1, 1, 30)); db.Pedidos.Add(pedido);
        await db.SaveChangesAsync(); db.ChangeTracker.Clear();
        UpdateStatusPedidoHandler Handler(CageContext context) => new(
            new PedidoRepository(context, NullLogger<EmpresaRepository>.Instance), NullLogger<UpdateStatusPedidoHandler>.Instance,
            new UpdateStatusPedidoValidator(TestData.Empresas().Object), new ReceitaRepository(context, NullLogger<ReceitaRepository>.Instance),
            new EstoqueRepository(context, NullLogger<EstoqueRepository>.Instance));
        Assert.True((await Handler(db).Handle(new() { PedidoId = 1, EmpresaId = 10, Status = "EmProducao" }, default)).IsSuccess);
        using var read = new CageContext(options);
        Assert.Equal(0, (await read.Estoque.FindAsync(1))!.QuantidadeAtual);
        Assert.Equal(1.3m, (await read.Estoque.FindAsync(2))!.QuantidadeAtual);
        var loaded = (await new PedidoRepository(read, NullLogger<EmpresaRepository>.Instance).GetByIdAsync(1, 10)).Value;
        Assert.Equal(2, loaded.ConsumosEstoque.Count);
        Assert.Equal(StatusPedidoEnum.EmProducao, loaded.StatusPedido);
        var receitaAtual = (await new ReceitaRepository(read, NullLogger<ReceitaRepository>.Instance).GetReceitaByIdAsync(1, 10)).Value;
        receitaAtual.Ingredientes.Single().AtualizarQuantidade(0.5m);
        await read.SaveChangesAsync();
        Assert.True((await Handler(read).Handle(new() { PedidoId = 1, EmpresaId = 10, Status = "Pendente" }, default)).IsSuccess);
        using var verify = new CageContext(options);
        Assert.Equal(0.3m, (await verify.Estoque.FindAsync(1))!.QuantidadeAtual);
        Assert.Equal(2, (await verify.Estoque.FindAsync(2))!.QuantidadeAtual);
        Assert.Empty(await verify.Set<ConsumoEstoquePedido>().ToListAsync());
        // Um novo ciclo cria novos registros, sem reaproveitar a devolução anterior.
        Assert.True((await Handler(read).Handle(new() { PedidoId = 1, EmpresaId = 10, Status = "EmProducao" }, default)).IsSuccess);
        using var final = new CageContext(options);
        Assert.Equal(2, await final.Set<ConsumoEstoquePedido>().CountAsync());
        Assert.Equal(1.8m, (await final.Estoque.FindAsync(2))!.QuantidadeAtual);
    }
}
