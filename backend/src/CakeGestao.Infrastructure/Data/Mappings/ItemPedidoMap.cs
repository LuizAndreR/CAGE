using CakeGestao.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CakeGestao.Infrastructure.Data.Mappings;

internal class ItemPedidoMap : IEntityTypeConfiguration<ItemPedido>
{
    public void Configure(EntityTypeBuilder<ItemPedido> builder)
    {
        builder.ToTable("ItensPedido");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantidade)
            .IsRequired();

        builder.Property(x => x.ValorUnitario)
            .IsRequired();

        builder.HasOne(i => i.Pedido)
            .WithMany(p => p.Itens)
            .HasForeignKey(i => i.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Receita) 
            .WithMany(r => r.ItemPedidos) 
            .HasForeignKey(i => i.ReceitaId) 
            .OnDelete(DeleteBehavior.Restrict);
    }
}
