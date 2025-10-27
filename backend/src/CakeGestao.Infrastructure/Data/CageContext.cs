using CakeGestao.Domain.Entities;
using CakeGestao.Infrastructure.Data.Mappings;
using Microsoft.EntityFrameworkCore;

namespace CakeGestao.Infrastructure.Data;

public class CageContext : DbContext
{
    public CageContext(DbContextOptions<CageContext> options) : base(options)
    {
    }

    public DbSet<Pedido> Pedidos { get; set; }
    public DbSet<Receita> Receitas { get; set; }
    public DbSet<ItemEstoque> ItensEstoque { get; set; }
    public DbSet<ItemPedido> ItensPedido { get; set; }
    public DbSet<Ingrediente> Ingredientes { get; set; }
    public DbSet<TransacaoFinanceira> TransacoesFinanceiras { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IngredienteMap).Assembly);
    }
}
