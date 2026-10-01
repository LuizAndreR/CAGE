using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Enum;
using CakeGestao.Infrastructure.Data;
using CakeGestao.Infrastructure.Data.Repositories;
using CakeGestao.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace CakeGestao.Tests.Repositories;

// Integração com EF Core InMemory: não exercita SQL, constraints ou migrations PostgreSQL.
public class RepositoryTests : IDisposable
{
    private readonly DbContextOptions<CageContext> _options = new DbContextOptionsBuilder<CageContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
    private readonly CageContext _db;
    public RepositoryTests() => _db = new(_options);
    public void Dispose() => _db.Dispose();
    private CageContext Reload() => new(_options);

    [Fact]
    public async Task EmpresaPersisteCriacaoAtualizacaoEExclusao()
    {
        var repo = new EmpresaRepository(_db, NullLogger<EmpresaRepository>.Instance);
        var empresa = TestData.Empresa();
        await repo.CreateEmpresaAsync(empresa);
        using (var read = Reload()) Assert.Equal("Confeitaria", (await read.Empresas.FindAsync(10))!.Nome);
        empresa.AtualizarDadosCadastrais("Novo nome", "Rua B");
        await repo.UpdateEmpresaAsync(empresa);
        using (var read = Reload()) Assert.Equal("Rua B", (await read.Empresas.FindAsync(10))!.Endereco);
        Assert.True((await repo.EmpresaExistsByNomeAsync("Novo nome")).IsFailed);
        Assert.True((await repo.EmpresaExistsByNomeAsync("Inexistente")).IsSuccess);
        Assert.Single((await repo.GetAllEmpresasAsync()).Value);
        await repo.DeleteEmpresaAsync(empresa);
        using (var read = Reload()) Assert.Null(await read.Empresas.FindAsync(10));
        Assert.True((await repo.GetEmpresaByIdAsync(10)).IsFailed);
    }

    [Fact]
    public async Task UsuarioPersisteAlteracoesEFiltraEmpresa()
    {
        var repo = new UsuarioRepository(_db, NullLogger<UsuarioRepository>.Instance);
        var usuario = TestData.Usuario();
        await repo.CreateUserAsync(usuario);
        await repo.CreateUserAsync(TestData.Usuario(8, 20));
        Assert.Equal(7, Assert.Single((await repo.GetAllUsuariosAsync(10)).Value).Id);
        Assert.Equal(2, (await repo.GetAllUsuariosAsync(null)).Value.Count);
        Assert.True((await repo.GetByIdAsync(8, 10)).IsFailed);
        Assert.True((await repo.GetUsuarioByEmailAsync("inexistente@example.com")).IsFailed);
        usuario.AtualizarFuncionario("Novo nome", UserRole.Confeiteiro);
        await repo.UpdateUsuarioAsync(usuario);
        using (var read = Reload()) Assert.Equal(UserRole.Confeiteiro, (await read.Usuarios.FindAsync(7))!.Role);
        await repo.DeleteAsync(usuario);
        using (var read = Reload()) { Assert.Null(await read.Usuarios.FindAsync(7)); Assert.NotNull(await read.Usuarios.FindAsync(8)); }
    }

    [Fact]
    public async Task EstoquePersisteQuantidadeECustoMedio()
    {
        var repo = new EstoqueRepository(_db, NullLogger<EstoqueRepository>.Instance);
        var item = TestData.Estoque();
        await repo.CreateItemEstoqueAsync(item);
        item.AdicionarQuantidade(10, 150);
        await repo.UpdateItemEstoqueAsync(item);
        using (var read = Reload())
        {
            var persisted = await read.Estoque.FindAsync(1);
            Assert.Equal(20, persisted!.QuantidadeAtual);
            Assert.Equal(10, persisted.ValorMedia);
        }
        await repo.DeleteItemEstoqueAsync(item);
        using (var read = Reload()) Assert.Null(await read.Estoque.FindAsync(1));
    }

    [Fact]
    public async Task EstoqueFiltraListagemIdsAlertasEDuplicidadePorEmpresa()
    {
        _db.Estoque.AddRange(TestData.Estoque(1, 10, 2), TestData.Estoque(2, 10, 3), TestData.Estoque(3, 20, 1));
        await _db.SaveChangesAsync();
        var repo = new EstoqueRepository(_db, NullLogger<EstoqueRepository>.Instance);
        Assert.Equal(2, await repo.GetTotalItensEstoqueAsync(10, default));
        Assert.Equal(2, (await repo.GetAllItemEstoqueByEmpresaIdAsync(10)).Value.Count);
        Assert.Equal(1, Assert.Single((await repo.GetItensByIdsAsync(new[] { 1, 3 }, 10)).Value).Id);
        Assert.Equal(1, Assert.Single((await repo.GetAlertaEstoqueByEmpresaIdAsync(10, 2)).Value).Id);
        Assert.True((await repo.GetItemEstoqueByIdAsync(3, 10)).IsFailed);
        Assert.True((await repo.ExistItemByNome(" FARINHA ", 10, " MARCA A ")).IsSuccess);
        Assert.True((await repo.ExistItemByNome("Farinha", 99, "Marca A")).IsFailed);
        Assert.True((await repo.ExistItemByNome("Farinha", 10, "Outra marca")).IsFailed);
    }

    [Fact]
    public async Task ReceitaCarregaIngredientesEItemEIdentificaDependencia()
    {
        _db.Estoque.Add(TestData.Estoque());
        var receita = TestData.Receita();
        receita.AdicionarIngrediente(new(1, 2, UnidadeMedidaEnum.KG));
        var repo = new ReceitaRepository(_db, NullLogger<ReceitaRepository>.Instance);
        Assert.True((await repo.CreateReceitaAsync(receita)).IsSuccess);
        _db.ChangeTracker.Clear();
        var loaded = (await repo.GetReceitaByIdAsync(1, 10)).Value;
        var ingrediente = Assert.Single(loaded.Ingredientes);
        Assert.Equal("Farinha", ingrediente.Item.Nome);
        Assert.Equal(2, ingrediente.Quantidade);
        var estoqueRepo = new EstoqueRepository(_db, NullLogger<EstoqueRepository>.Instance);
        Assert.True(await estoqueRepo.ExisteDependenciaComReceitaAsync(1, 10));
        Assert.False(await estoqueRepo.ExisteDependenciaComReceitaAsync(1, 20));
    }

    [Fact]
    public async Task ReceitaFiltraEmpresaOrdenaRecentesEPersisteStatus()
    {
        var repo = new ReceitaRepository(_db, NullLogger<ReceitaRepository>.Instance);
        await repo.CreateReceitaAsync(TestData.Receita(1));
        await repo.CreateReceitaAsync(TestData.Receita(2));
        await repo.CreateReceitaAsync(TestData.Receita(3, 20));
        Assert.Equal(2, Assert.Single(await repo.GetUltimasReceitasAsync(10, 1, default)).Id);
        Assert.Equal(2, (await repo.GetAllReceitasAsync(10)).Value.Count);
        Assert.True((await repo.GetReceitaByIdAsync(3, 10)).IsFailed);
        var receita = (await repo.GetReceitaByIdAsync(1, 10)).Value;
        receita.AlteraStatus(false);
        await repo.UpdateReceitaAsync(receita);
        using (var read = Reload()) Assert.False((await read.Receitas.FindAsync(1))!.Status);
        await repo.DeleteReceitaAsync(receita);
        using (var read = Reload()) Assert.Null(await read.Receitas.FindAsync(1));
    }

    [Fact]
    public async Task PedidoCarregaItensEReceitaEPersistePagamento()
    {
        _db.Receitas.Add(TestData.Receita());
        var pedido = TestData.Pedido();
        pedido.AdicionarItem(new(1, 2, 30));
        var repo = new PedidoRepository(_db, NullLogger<EmpresaRepository>.Instance);
        await repo.CreatePedidoAsync(pedido);
        _db.ChangeTracker.Clear();
        var loaded = (await repo.GetByIdAsync(1, 10)).Value;
        Assert.Equal("Bolo", Assert.Single(loaded.Itens).Receita.Nome);
        Assert.Equal(60, loaded.ValorTotal);
        loaded.InformarPagamento();
        await repo.UpdatePedidoAsync(loaded);
        using (var read = Reload()) Assert.True((await read.Pedidos.FindAsync(1))!.Pago);
        await repo.DeletePedidoAsync(loaded);
        using (var read = Reload()) Assert.Null(await read.Pedidos.FindAsync(1));
    }

    [Fact]
    public async Task PedidoListaMaisRecentesDaEmpresa()
    {
        var antigo = TestData.Pedido(1);
        var recente = TestData.Pedido(2);
        _db.Pedidos.AddRange(antigo, recente, TestData.Pedido(3, 20));
        _db.Entry(antigo).Property(p => p.DataCriacao).CurrentValue = new DateTime(2025, 1, 1);
        _db.Entry(recente).Property(p => p.DataCriacao).CurrentValue = new DateTime(2025, 2, 1);
        await _db.SaveChangesAsync();
        var repo = new PedidoRepository(_db, NullLogger<EmpresaRepository>.Instance);
        Assert.Equal(new[] { 2, 1 }, (await repo.GetAllByEmpresaIdAsync(10)).Value.Select(p => p.Id));
        Assert.Equal(2, Assert.Single(await repo.GetUltimosPedidosAsync(10, 1, default)).Id);
        Assert.True((await repo.GetByIdAsync(3, 10)).IsFailed);
    }

    [Fact]
    public async Task FinanceiroCombinaFiltrosEExcluiLimiteSuperiorDoMes()
    {
        _db.TransacoesFinanceiras.AddRange(
            TestData.Transacao(1, data: new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc)),
            TestData.Transacao(2, data: new DateTime(2026, 5, 31, 23, 59, 59, DateTimeKind.Utc)),
            TestData.Transacao(3, data: new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc)),
            TestData.Transacao(4, tipo: TipoTransacaoEnum.Saida), TestData.Transacao(5, empresa: 20));
        await _db.SaveChangesAsync();
        var repo = new FinanceiroRepository(_db, NullLogger<FinanceiroRepository>.Instance);
        var result = await repo.GetAllTransacoesAsync(10, TipoTransacaoEnum.Entrada, CategoriasEnum.Vendas, 5, 2026);
        Assert.Equal(new[] { 2, 1 }, result.Value.Select(t => t.Id));
        Assert.True((await repo.GetTransacaoAsync(5, 10)).IsFailed);
        Assert.Equal(4, (await repo.GetAllTransacoesAsync(10, null, null, null, null)).Value.Count);
    }

    [Fact]
    public async Task FinanceiroPersisteCancelamentoEIgnoraCanceladasNosTotaisGerais()
    {
        var repo = new FinanceiroRepository(_db, NullLogger<FinanceiroRepository>.Instance);
        var cancelada = TestData.Transacao(2, valor: 999);
        await repo.CreateTransacaoAsync(TestData.Transacao(1, valor: 100));
        await repo.CreateTransacaoAsync(cancelada);
        await repo.CreateTransacaoAsync(TestData.Transacao(3, tipo: TipoTransacaoEnum.Saida, valor: 40));
        await repo.CreateTransacaoAsync(TestData.Transacao(4, empresa: 20, valor: 500));
        cancelada.Cancelar("Duplicada", 7);
        await repo.UpdateTransacaoAsync(cancelada);
        using (var read = Reload())
        {
            var persisted = await read.TransacoesFinanceiras.FindAsync(2);
            Assert.True(persisted!.IsCancelado);
            Assert.Equal("Duplicada", persisted.MotivoCancelamento);
            Assert.Equal(7, persisted.CanceladoPorUsuarioId);
        }
        Assert.Equal(100, (await repo.GetEntradaAsync(10)).Value);
        Assert.Equal(40, (await repo.GetSaidaAsync(10)).Value);
    }

    [Fact]
    public async Task TokenPersisteRotacaoEConsultaSemRastreamento()
    {
        var repo = new TokenRepository(_db, NullLogger<TokenRepository>.Instance);
        await repo.SaveRefreshTokenAsync(new() { RefreshToken = "token-teste", UsuarioId = 7, ExpiresAt = DateTime.UtcNow.AddDays(1) });
        _db.ChangeTracker.Clear();
        var token = (await repo.GetRefreshTokenAsync("token-teste")).Value;
        Assert.Empty(_db.ChangeTracker.Entries());
        token.IsUsed = true;
        token.IsRevoked = true;
        await repo.UpdateRefreshTokenAsync(token);
        using (var read = Reload())
        {
            var persisted = await read.TokensRefresh.SingleAsync();
            Assert.True(persisted.IsUsed && persisted.IsRevoked);
        }
        Assert.True((await repo.GetRefreshTokenAsync("inexistente")).IsFailed);
    }
}
