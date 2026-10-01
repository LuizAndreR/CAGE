using CakeGestao.Application.Features.Empresas.Query.Get;
using CakeGestao.Application.Features.Empresas.Query.GetAll;
using CakeGestao.Application.Features.User.Query.Get;
using CakeGestao.Application.Features.User.Query.GetAll;
using CakeGestao.Application.Features.User.Query.GetFuncionario;
using CakeGestao.Application.Features.Estoque.Query.GetItem;
using CakeGestao.Application.Features.Receitas.Query.GetReceita;
using CakeGestao.Application.Features.Receitas.Query.GetAll;
using CakeGestao.Application.Features.Pedidos.Query.Get;
using CakeGestao.Application.Features.Financeiro.Query.GetAll;
using CakeGestao.Application.Features.Financeiro.Query.GetResumo;
using CakeGestao.Application.Features.Dashboard.Query;
using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Exceptions;
using CakeGestao.Domain.Interfaces.Repositories;
using CakeGestao.Tests.Support;
using FluentResults;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CakeGestao.Tests.Queries;

public class QueryHandlerTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task EmpresaConsultaMapeiaDadosOuPropagaAusencia(bool exists)
    {
        var repo = new Mock<IEmpresaRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetEmpresaByIdAsync(10)).ReturnsAsync(exists ? Result.Ok(TestData.Empresa()) : Result.Fail<Empresa>("Ausente"));
        var handler = new GetEmpresaHandler(repo.Object, NullLogger<GetEmpresaHandler>.Instance, TestData.Mapper());
        var result = await handler.Handle(new() { Id = 10 }, default);
        Assert.Equal(exists, result.IsSuccess);
        if (exists) { Assert.Equal("Confeitaria", result.Value.Nome); Assert.Equal(10, result.Value.Id); }
        else Assert.Equal("Ausente", Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task EmpresaListagemMapeiaTodasAsEmpresas()
    {
        var repo = new Mock<IEmpresaRepository>();
        repo.Setup(r => r.GetAllEmpresasAsync()).ReturnsAsync(Result.Ok(new List<Empresa> { TestData.Empresa(10), TestData.Empresa(20) }));
        var result = await new GetAllEmpresaHandler(repo.Object, NullLogger<GetAllEmpresaHandler>.Instance, TestData.Mapper()).Handle(new(), default);
        Assert.Equal(new[] { 10, 20 }, result.Value.Select(e => e.Id));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UsuarioConsultaRespeitaEmpresaEMapeiaDataDeInicio(bool exists)
    {
        var user = TestData.Usuario();
        var repo = new Mock<IUsuarioRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetByIdAsync(7, 10)).ReturnsAsync(exists ? Result.Ok(user) : Result.Fail<Usuario>("Ausente"));
        var result = await new GetUsuarioHandler(NullLogger<GetUsuarioHandler>.Instance, repo.Object, TestData.Mapper()).Handle(new() { Id = 7, EmpresaId = 10 }, default);
        Assert.Equal(exists, result.IsSuccess);
        if (exists) { Assert.Equal(user.DataCriacao, result.Value.DataInicio); Assert.Equal("Caixa", result.Value.Role); Assert.Equal(user.Email, result.Value.Email); }
        else Assert.IsType<NotFoundError>(Assert.Single(result.Errors));
    }

    [Fact]
    public async Task UsuarioListagemAdministrativaExcluiAdmins()
    {
        var repo = new Mock<IUsuarioRepository>();
        repo.Setup(r => r.GetAllUsuariosAsync(null)).ReturnsAsync(Result.Ok(new List<Usuario> { TestData.Usuario(), TestData.Usuario(8, role: UserRole.Admin) }));
        var result = await new GetAllUsuarioHandler(repo.Object, TestData.Mapper(), NullLogger<GetAllUsuarioHandler>.Instance).Handle(new(), default);
        Assert.Equal(7, Assert.Single(result.Value).Id);
    }

    [Fact]
    public async Task FuncionariosListagemUsaEmpresaInformada()
    {
        var repo = new Mock<IUsuarioRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetAllUsuariosAsync(10)).ReturnsAsync(Result.Ok(new List<Usuario> { TestData.Usuario() }));
        var result = await new GetFuncionariosHandler(NullLogger<GetFuncionariosHandler>.Instance,
            new GetFuncionariosValidator(TestData.Empresas().Object), repo.Object, TestData.Mapper()).Handle(new() { EmpresaId = 10 }, default);
        Assert.Equal(7, Assert.Single(result.Value).Id);
        repo.Verify(r => r.GetAllUsuariosAsync(10), Times.Once);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task EstoqueConsultaMapeiaQuantidadeOuRetornaAusencia(bool exists)
    {
        var repo = new Mock<IEstoqueRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetItemEstoqueByIdAsync(1, 10)).ReturnsAsync(exists ? Result.Ok(TestData.Estoque()) : Result.Fail<Estoque>("Ausente"));
        var result = await new GetItemEstoqueHandler(repo.Object, NullLogger<GetItemEstoqueHandler>.Instance, TestData.Mapper(),
            new GetItemEstoqueValidator(TestData.Empresas().Object)).Handle(new() { EmpresaId = 10, ItemId = 1 }, default);
        Assert.Equal(exists, result.IsSuccess);
        if (exists) { Assert.Equal("Farinha", result.Value.Nome); Assert.Equal(10, result.Value.QuantidadeAtual); }
        else Assert.IsType<NotFoundError>(Assert.Single(result.Errors));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ReceitaConsultaMapeiaPrecoECustoExtraOuRetornaAusencia(bool exists)
    {
        var repo = new Mock<IReceitaRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetReceitaByIdAsync(1, 10)).ReturnsAsync(exists ? Result.Ok(TestData.Receita()) : Result.Fail<Receita>("Ausente"));
        var result = await new GetReceitaHandler(NullLogger<GetReceitaHandler>.Instance, repo.Object,
            new GetReceitaValidator(TestData.Empresas().Object), TestData.Mapper()).Handle(new() { EmpresaId = 10, Id = 1 }, default);
        Assert.Equal(exists, result.IsSuccess);
        if (exists) { Assert.Equal(30, result.Value.PrecoVenda); Assert.Equal(5, result.Value.CustoExtra); Assert.True(result.Value.Status); }
        else Assert.IsType<NotFoundError>(Assert.Single(result.Errors));
    }

    [Fact]
    public async Task ReceitaIdInvalidoNaoConsultaRepositorio()
    {
        var repo = new Mock<IReceitaRepository>(MockBehavior.Strict);
        var result = await new GetReceitaHandler(NullLogger<GetReceitaHandler>.Instance, repo.Object,
            new GetReceitaValidator(TestData.Empresas().Object), TestData.Mapper()).Handle(new() { EmpresaId = 10, Id = 0 }, default);
        Assert.IsType<ValidationError>(Assert.Single(result.Errors)); repo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ReceitaListagemVaziaRetornaNaoEncontrado()
    {
        var repo = new Mock<IReceitaRepository>();
        repo.Setup(r => r.GetAllReceitasAsync(10)).ReturnsAsync(Result.Ok(new List<Receita>()));
        var result = await new GetAllReceitaHandler(repo.Object, TestData.Mapper(), new GetAllReceitaValidator(TestData.Empresas().Object),
            NullLogger<GetAllReceitaHandler>.Instance).Handle(new() { EmpresaId = 10 }, default);
        Assert.IsType<NotFoundError>(Assert.Single(result.Errors));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task PedidoDetalheIncluiNomeDaReceitaSubtotalETotal(bool exists)
    {
        var pedido = TestData.Pedido();
        pedido.AdicionarItem(new(1, 2, 30) { Receita = TestData.Receita() });
        var repo = new Mock<IPedidoRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetByIdAsync(1, 10)).ReturnsAsync(exists ? Result.Ok(pedido) : Result.Fail<Pedido>("Ausente"));
        var result = await new GetPedidoByIdHandler(repo.Object, new GetPedidoByIdValidator(TestData.Empresas().Object),
            NullLogger<GetPedidoByIdHandler>.Instance, TestData.Mapper()).Handle(new() { EmpresaId = 10, Id = 1 }, default);
        Assert.Equal(exists, result.IsSuccess);
        if (!exists) { Assert.IsType<NotFoundError>(Assert.Single(result.Errors)); return; }
        var item = Assert.Single(result.Value.Itens);
        Assert.Equal("Bolo", item.NomeReceita); Assert.Equal(60, item.SubTotal); Assert.Equal(60, result.Value.ValorTotal);
        Assert.Equal("Pendente", result.Value.StatusPedido);
    }

    [Theory]
    [InlineData(0, 2025)]
    [InlineData(13, 2025)]
    [InlineData(5, null)]
    [InlineData(null, 2025)]
    public async Task FinanceiroRejeitaPeriodoInvalidoAntesDaConsulta(int? mes, int? ano)
    {
        var repo = new Mock<IFinanceiroRepository>(MockBehavior.Strict);
        var handler = new GetAllTransacaoHandler(repo.Object, TestData.Mapper(), NullLogger<GetAllTransacaoHandler>.Instance,
            new GetAllTransacaoValidator(TestData.Empresas().Object));
        var result = await handler.Handle(new() { EmpresaId = 10, Tipo = "Entrada", Categoria = "Vendas", Mes = mes, Ano = ano }, default);
        Assert.IsType<ValidationError>(Assert.Single(result.Errors)); repo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task FinanceiroConverteFiltrosSemDiferenciarMaiusculas()
    {
        var repo = new Mock<IFinanceiroRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetAllTransacoesAsync(10, TipoTransacaoEnum.Entrada, CategoriasEnum.Vendas, 5, 2025))
            .ReturnsAsync(Result.Ok(new List<TransacaoFinanceira> { TestData.Transacao() }));
        var result = await new GetAllTransacaoHandler(repo.Object, TestData.Mapper(), NullLogger<GetAllTransacaoHandler>.Instance,
            new GetAllTransacaoValidator(TestData.Empresas().Object)).Handle(new() { EmpresaId = 10, Tipo = "entrada", Categoria = "vendas", Mes = 5, Ano = 2025 }, default);
        Assert.Equal(100, Assert.Single(result.Value).Valor);
        repo.VerifyAll();
    }

    [Fact]
    public async Task ResumoFinanceiroRetornaEntradasESaidasDaEmpresa()
    {
        var repo = new Mock<IFinanceiroRepository>(MockBehavior.Strict);
        using var cts = new CancellationTokenSource();
        repo.Setup(r => r.GetTotalPorTipoMesAtualAsync(TipoTransacaoEnum.Entrada, 10, cts.Token)).ReturnsAsync(150);
        repo.Setup(r => r.GetTotalPorTipoMesAtualAsync(TipoTransacaoEnum.Saida, 10, cts.Token)).ReturnsAsync(40);
        var result = await new GetFinanceiroResumoHandler(NullLogger<GetFinanceiroResumoHandler>.Instance, repo.Object,
            new GetFinanceiroResumoValidator(TestData.Empresas().Object)).Handle(new() { EmpresaId = 10 }, cts.Token);
        Assert.Equal(150, result.Value.Entrada); Assert.Equal(40, result.Value.Saida); repo.VerifyAll();
    }

    [Theory]
    [InlineData(150, 40, 110)]
    [InlineData(40, 150, -110)]
    [InlineData(0, 0, 0)]
    public async Task DashboardCalculaSaldoEMapeiaUltimosRegistros(int entrada, int saida, int saldo)
    {
        var estoque = new Mock<IEstoqueRepository>(MockBehavior.Strict);
        var financeiro = new Mock<IFinanceiroRepository>(MockBehavior.Strict);
        var receitas = new Mock<IReceitaRepository>(MockBehavior.Strict);
        var pedidos = new Mock<IPedidoRepository>(MockBehavior.Strict);
        estoque.Setup(r => r.GetTotalItensEstoqueAsync(10, default)).ReturnsAsync(4);
        financeiro.Setup(r => r.GetTotalPorTipoMesAtualAsync(TipoTransacaoEnum.Entrada, 10, default)).ReturnsAsync(entrada);
        financeiro.Setup(r => r.GetTotalPorTipoMesAtualAsync(TipoTransacaoEnum.Saida, 10, default)).ReturnsAsync(saida);
        receitas.Setup(r => r.GetUltimasReceitasAsync(10, 5, default)).ReturnsAsync(new[] { TestData.Receita() });
        pedidos.Setup(r => r.GetUltimosPedidosAsync(10, 5, default)).ReturnsAsync(new[] { TestData.Pedido() });
        var result = await new GetDashboardResumoHandler(estoque.Object, financeiro.Object, receitas.Object, pedidos.Object,
            NullLogger<GetDashboardResumoHandler>.Instance).Handle(new() { EmpresaId = 10, NomeUsuario = "Ana" }, default);
        Assert.Equal("Ana", result.Value.NomeUsuario); Assert.Equal(4, result.Value.TotalItensEstoque);
        Assert.Equal(saldo, result.Value.Financeiro.SaldoAtual);
        Assert.Equal("Bolo", Assert.Single(result.Value.UltimasReceitas).Nome);
        Assert.Equal("Pendente", Assert.Single(result.Value.UltimosPedidos).Status);
    }
}
