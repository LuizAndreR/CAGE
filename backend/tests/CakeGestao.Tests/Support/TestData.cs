using AutoMapper;
using CakeGestao.Application.Mappings;
using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace CakeGestao.Tests.Support;

internal static class TestData
{
    public static Empresa Empresa(int id = 10) => new("Confeitaria", "Rua A", StatusEmpresaEnum.Ativa) { Id = id };
    public static Usuario Usuario(int id = 7, int empresa = 10, UserRole role = UserRole.Caixa) =>
        new("Ana", "ana@example.com", "hash", role, new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc), empresa) { Id = id };
    public static Estoque Estoque(int id = 1, int empresa = 10, decimal quantidade = 10) =>
        new("Farinha", "Marca A", quantidade, UnidadeMedidaEnum.KG, 2, 5, empresa, null, null) { Id = id };
    public static Receita Receita(int id = 1, int empresa = 10)
    {
        var receita = new Receita("Bolo", "Assar", empresa) { Id = id };
        receita.CalcularPrecificacao(10, 5, 100, 30);
        return receita;
    }
    public static Pedido Pedido(int id = 1, int empresa = 10) => new("Cliente", "85999999999", "Aniversário", null, empresa) { Id = id };
    public static TransacaoFinanceira Transacao(int id = 1, int empresa = 10, TipoTransacaoEnum tipo = TipoTransacaoEnum.Entrada,
        decimal valor = 100, DateTime? data = null) =>
        new(tipo, tipo == TipoTransacaoEnum.Entrada ? CategoriasEnum.Vendas : CategoriasEnum.Compras,
            valor, data ?? new DateTime(2026, 5, 15, 0, 0, 0, DateTimeKind.Utc), "Movimentação", null, empresa) { Id = id };
    public static Mock<IEmpresaRepository> Empresas()
    {
        var repo = new Mock<IEmpresaRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetEmpresaByIdAsync(10)).ReturnsAsync(Result.Ok(Empresa()));
        return repo;
    }
    public static IMapper Mapper() => new MapperConfiguration(c => c.AddMaps(typeof(UsuarioProfile).Assembly), Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance).CreateMapper();
    public static ControllerContext Context(string? empresa = "10", string role = "Dono")
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "7"), new(ClaimTypes.Name, "Ana"), new(ClaimTypes.Role, role) };
        if (empresa != null) claims.Add(new("EmpresaId", empresa));
        return new() { HttpContext = new DefaultHttpContext { User = new(new ClaimsIdentity(claims, "Tests")) } };
    }
}
