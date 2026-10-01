using CakeGestao.Application.Features.Empresas.Command.Create;
using CakeGestao.Application.Features.Empresas.Command.Update;
using CakeGestao.Application.Features.Empresas.Create;
using CakeGestao.Application.UseCases.Empresas.UseCase;
using CakeGestao.Application.Features.Estoque.Command.AddQuantidade;
using CakeGestao.Application.Features.Estoque.Command.RemoverQuantidade;
using CakeGestao.Application.Features.Estoque.Command.Delete;
using CakeGestao.Application.Features.Pedidos.Command.Create;
using CakeGestao.Application.Features.Financeiro.Command.Create;
using CakeGestao.Application.Features.Financeiro.Command.Cancelamento;
using CakeGestao.Application.Features.Receitas.Command.Status;
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

public class BusinessCommandTests
{
    [Theory]
    [InlineData("", "Rua A")]
    [InlineData("Empresa", "")]
    public async Task EmpresaInvalidaNaoConsultaNemGrava(string nome, string endereco)
    {
        var repo = new Mock<IEmpresaRepository>(MockBehavior.Strict);
        var handler = new CreateEmpresaHandler(repo.Object, NullLogger<CreateEmpresaHandler>.Instance, new CreateEmpresaValidator());
        var result = await handler.Handle(new() { Nome = nome, Endereco = endereco }, default);
        Assert.IsType<ValidationError>(Assert.Single(result.Errors)); repo.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task EmpresaDuplicadaNaoGravaENovaEmpresaNasceAtiva(bool duplicate)
    {
        var repo = new Mock<IEmpresaRepository>();
        repo.Setup(r => r.EmpresaExistsByNomeAsync("Empresa")).ReturnsAsync(duplicate ? Result.Fail("Duplicada") : Result.Ok());
        var handler = new CreateEmpresaHandler(repo.Object, NullLogger<CreateEmpresaHandler>.Instance, new CreateEmpresaValidator());
        var result = await handler.Handle(new() { Nome = "Empresa", Endereco = "Rua A" }, default);
        Assert.Equal(!duplicate, result.IsSuccess);
        if (duplicate) Assert.IsType<ConflictError>(Assert.Single(result.Errors));
        repo.Verify(r => r.CreateEmpresaAsync(It.Is<Empresa>(e => e.Nome == "Empresa" && e.Endereco == "Rua A" && e.Status == StatusEmpresaEnum.Ativa)), duplicate ? Times.Never() : Times.Once());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AtualizacaoEmpresaTrataRegistroAusenteEPreservaStatus(bool exists)
    {
        var entity = TestData.Empresa();
        var repo = new Mock<IEmpresaRepository>();
        repo.Setup(r => r.GetEmpresaByIdAsync(10)).ReturnsAsync(exists ? Result.Ok(entity) : Result.Fail<Empresa>("Ausente"));
        var handler = new UpdateEmpresaHandler(repo.Object, new UpdateEmpresaValidator(), NullLogger<UpdateEmpresaHandler>.Instance);
        var result = await handler.Handle(new() { Id = 10, Nome = "Novo nome", Endereco = "Rua B" }, default);
        Assert.Equal(exists, result.IsSuccess);
        if (!exists) Assert.IsType<NotFoundError>(Assert.Single(result.Errors));
        repo.Verify(r => r.UpdateEmpresaAsync(It.Is<Empresa>(e => e.Id == 10 && e.Nome == "Novo nome" && e.Endereco == "Rua B" && e.Status == StatusEmpresaEnum.Ativa)), exists ? Times.Once() : Times.Never());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task EntradaEstoqueCalculaCustoMedioESoGravaSeFinanceiroAceitar(bool financeiroOk)
    {
        var repo = new Mock<IEstoqueRepository>();
        var item = TestData.Estoque();
        repo.Setup(r => r.GetItemEstoqueByIdAsync(1, 10)).ReturnsAsync(Result.Ok(item));
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<CreateTransacaoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(financeiroOk ? Result.Ok() : Result.Fail("Indisponível"));
        var handler = new AddQuantidadeEstoqueHandler(repo.Object, NullLogger<AddQuantidadeEstoqueHandler>.Instance,
            new AddQuantidadeEstoqueValidator(TestData.Empresas().Object), mediator.Object);
        var result = await handler.Handle(new() { EmpresaId = 10, ItemId = 1, QuantidadeAdicionar = 10, Valor = 150 }, default);
        Assert.Equal(financeiroOk, result.IsSuccess);
        mediator.Verify(m => m.Send(It.Is<CreateTransacaoCommand>(c => c.EmpresaId == 10 && c.Tipo == "Saida" && c.Categoria == "Compras" && c.Valor == 150), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.UpdateItemEstoqueAsync(It.Is<Estoque>(e => e.QuantidadeAtual == 20 && e.ValorMedia == 10)), financeiroOk ? Times.Once() : Times.Never());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task EntradaEstoqueRejeitaQuantidadeNaoPositiva(int quantity)
    {
        var repo = new Mock<IEstoqueRepository>(MockBehavior.Strict);
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        var handler = new AddQuantidadeEstoqueHandler(repo.Object, NullLogger<AddQuantidadeEstoqueHandler>.Instance,
            new AddQuantidadeEstoqueValidator(TestData.Empresas().Object), mediator.Object);
        Assert.IsType<ValidationError>(Assert.Single((await handler.Handle(new() { EmpresaId = 10, ItemId = 1, QuantidadeAdicionar = quantity, Valor = 10 }, default)).Errors));
        repo.VerifyNoOtherCalls(); mediator.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task BaixaEstoqueTrataAusenciaEAtualizaQuantidade(bool exists)
    {
        var repo = new Mock<IEstoqueRepository>();
        repo.Setup(r => r.GetItemEstoqueByIdAsync(1, 10)).ReturnsAsync(exists ? Result.Ok(TestData.Estoque()) : Result.Fail<Estoque>("Ausente"));
        var handler = new RemoverQuantidadeEstoqueHandler(repo.Object, NullLogger<RemoverQuantidadeEstoqueHandler>.Instance,
            new RemoverQuantidadeEstoqueValidator(TestData.Empresas().Object));
        var result = await handler.Handle(new() { EmpresaId = 10, ItemId = 1, QuantidadeARemover = 3 }, default);
        Assert.Equal(exists, result.IsSuccess);
        if (!exists) Assert.IsType<NotFoundError>(Assert.Single(result.Errors));
        repo.Verify(r => r.UpdateItemEstoqueAsync(It.Is<Estoque>(e => e.QuantidadeAtual == 7 && e.ValorMedia == 5)), exists ? Times.Once() : Times.Never());
    }

    [Fact]
    public async Task ExclusaoEstoqueEmUsoNaoRemoveItem()
    {
        var repo = new Mock<IEstoqueRepository>(MockBehavior.Strict);
        repo.Setup(r => r.ExisteDependenciaComReceitaAsync(1, 10)).ReturnsAsync(true);
        var handler = new DeleteItemEstoqueHandler(repo.Object, new DeleteItemEstoqueValidator(TestData.Empresas().Object), NullLogger<DeleteItemEstoqueHandler>.Instance);
        Assert.IsType<ValidationError>(Assert.Single((await handler.Handle(new() { EmpresaId = 10, ItemId = 1 }, default)).Errors));
        repo.Verify(r => r.DeleteItemEstoqueAsync(It.IsAny<Estoque>()), Times.Never);
    }

    [Theory]
    [InlineData("ausente")]
    [InlineData("inativa")]
    [InlineData("ativa")]
    public async Task PedidoUsaPrecoDaReceitaEBloqueiaReceitaInativaOuAusente(string scenario)
    {
        var receita = TestData.Receita(); receita.AlteraStatus(scenario != "inativa");
        var receitas = new Mock<IReceitaRepository>();
        receitas.Setup(r => r.GetReceitaByIdAsync(1, 10)).ReturnsAsync(scenario == "ausente" ? Result.Fail<Receita>("Ausente") : Result.Ok(receita));
        var pedidos = new Mock<IPedidoRepository>();
        pedidos.Setup(r => r.CreatePedidoAsync(It.IsAny<Pedido>())).ReturnsAsync(Result.Ok());
        var handler = new CreatePedidoHandler(NullLogger<CreatePedidoHandler>.Instance, new CreatePedidoValidator(TestData.Empresas().Object), receitas.Object, pedidos.Object);
        var result = await handler.Handle(new() { EmpresaId = 10, ClienteNome = "Cliente", Itens = new() { new() { ReceitaId = 1, Quantidade = 2 } } }, default);
        Assert.Equal(scenario == "ativa", result.IsSuccess);
        if (scenario == "ausente") Assert.IsType<NotFoundError>(Assert.Single(result.Errors));
        if (scenario == "inativa") Assert.IsType<ValidationError>(Assert.Single(result.Errors));
        pedidos.Verify(r => r.CreatePedidoAsync(It.Is<Pedido>(p => p.EmpresaId == 10 && p.ValorTotal == 60 && !p.Pago && p.Itens.Count == 1)), scenario == "ativa" ? Times.Once() : Times.Never());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task PedidoNaoAceitaQuantidadeInvalida(int quantity)
    {
        var receitas = new Mock<IReceitaRepository>(MockBehavior.Strict);
        var pedidos = new Mock<IPedidoRepository>(MockBehavior.Strict);
        var handler = new CreatePedidoHandler(NullLogger<CreatePedidoHandler>.Instance, new CreatePedidoValidator(TestData.Empresas().Object), receitas.Object, pedidos.Object);
        Assert.IsType<ValidationError>(Assert.Single((await handler.Handle(new() { EmpresaId = 10, ClienteNome = "Cliente", Itens = new() { new() { ReceitaId = 1, Quantidade = quantity } } }, default)).Errors));
        receitas.VerifyNoOtherCalls(); pedidos.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ReceitaPermiteAtivarEDesativar(bool status)
    {
        var repo = new Mock<IReceitaRepository>();
        repo.Setup(r => r.GetReceitaByIdAsync(1, 10)).ReturnsAsync(Result.Ok(TestData.Receita()));
        repo.Setup(r => r.UpdateReceitaAsync(It.IsAny<Receita>())).ReturnsAsync(Result.Ok());
        var handler = new UpdateReceitaStatusHandler(new UpdateReceitaStatusValidator(TestData.Empresas().Object), NullLogger<UpdateReceitaStatusHandler>.Instance, repo.Object);
        Assert.True((await handler.Handle(new() { EmpresaId = 10, ReceitaId = 1, Status = status }, default)).IsSuccess);
        repo.Verify(r => r.UpdateReceitaAsync(It.Is<Receita>(r => r.Id == 1 && r.Status == status)), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(150)]
    public async Task TransacaoExigeValorPositivoEPreservaDados(int value)
    {
        var repo = new Mock<IFinanceiroRepository>();
        var handler = new CreateTransacaoHandler(repo.Object, NullLogger<CreateTransacaoHandler>.Instance, new CreateTransacaoValidator(TestData.Empresas().Object));
        var date = DateTime.UtcNow.AddDays(-1);
        var result = await handler.Handle(new() { EmpresaId = 10, Tipo = "Saida", Categoria = "Compras", Valor = value, Data = date, Descricao = "Ingredientes" }, default);
        Assert.Equal(value > 0, result.IsSuccess);
        if (value <= 0) Assert.IsType<ValidationError>(Assert.Single(result.Errors));
        repo.Verify(r => r.CreateTransacaoAsync(It.Is<TransacaoFinanceira>(t => t.EmpresaId == 10 && t.Tipo == TipoTransacaoEnum.Saida && t.Valor == value && t.Data == date && t.Descricao == "Ingredientes")), value > 0 ? Times.Once() : Times.Never());
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("Duplicada", false)]
    [InlineData("Duplicada", true)]
    public async Task CancelamentoValidaMotivoERegistraResponsavel(string motivo, bool exists)
    {
        var repo = new Mock<IFinanceiroRepository>();
        repo.Setup(r => r.GetTransacaoAsync(1, 10)).ReturnsAsync(exists ? Result.Ok(TestData.Transacao()) : Result.Fail<TransacaoFinanceira>("Ausente"));
        var handler = new CancelTransacaoHandler(repo.Object, new CancelTransacaoValidator(TestData.Empresas().Object), NullLogger<CancelTransacaoHandler>.Instance);
        var result = await handler.Handle(new() { EmpresaId = 10, TransacaoId = 1, UsuarioId = 7, Motivo = motivo }, default);
        Assert.Equal(exists && motivo.Length > 0, result.IsSuccess);
        if (motivo.Length == 0) Assert.IsType<ValidationError>(Assert.Single(result.Errors));
        else if (!exists) Assert.IsType<NotFoundError>(Assert.Single(result.Errors));
        repo.Verify(r => r.UpdateTransacaoAsync(It.Is<TransacaoFinanceira>(t => t.IsCancelado && t.MotivoCancelamento == motivo && t.CanceladoPorUsuarioId == 7 && t.DataCancelamento != null)), exists ? Times.Once() : Times.Never());
    }
}
