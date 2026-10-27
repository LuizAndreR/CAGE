using CakeGestao.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CakeGestao.Infrastructure.Data.Mappings;

internal class ConsumoEstoquePedidoMap : IEntityTypeConfiguration<ConsumoEstoquePedido>
{
    public void Configure(EntityTypeBuilder<ConsumoEstoquePedido> builder)
    {
        builder.ToTable("ConsumosEstoquePedido");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantidade).HasPrecision(18, 6);
        builder.HasOne<Pedido>().WithMany(x => x.ConsumosEstoque)
            .HasForeignKey(x => x.PedidoId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Estoque>().WithMany()
            .HasForeignKey(x => x.ItemId).OnDelete(DeleteBehavior.Restrict);
    }
}
