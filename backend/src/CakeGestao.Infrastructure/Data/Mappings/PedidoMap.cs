using CakeGestao.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CakeGestao.Infrastructure.Data.Mappings;

internal class PedidoMap : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("Pedidos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ClienteNome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Descricao)
            .HasMaxLength(1000);

        builder.Property(x => x.DataPedido)
            .IsRequired();

        builder.Property(x => x.DataEntrega)
            .IsRequired();

        builder.Property(x => x.ValorTotal)
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.HasOne(x => x.Empresa)
            .WithMany(x => x.Pedidos)
            .HasForeignKey(x => x.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
