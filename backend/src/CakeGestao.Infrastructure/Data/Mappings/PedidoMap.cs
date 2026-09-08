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

        builder.Property(x => x.TelefoneCliente)
            .HasMaxLength(20);

        builder.Property(x => x.Descricao)
            .HasMaxLength(1000);

        builder.Property(x => x.DataCriacao)
            .IsRequired();

        builder.Property(x => x.DataEntrega)
            .IsRequired(false);

        builder.Property(x => x.ValorTotal)
             .IsRequired()
             .HasColumnType("decimal(10,2)")
             .HasDefaultValue(0);

        builder.Property(x => x.StatusPedido)
            .IsRequired();

        builder.Property(x => x.Pago)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.DataPagamento)
            .IsRequired(false);

        builder.HasOne(x => x.Empresa)
            .WithMany(x => x.Pedidos)
            .HasForeignKey(x => x.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Pedido.Itens))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
