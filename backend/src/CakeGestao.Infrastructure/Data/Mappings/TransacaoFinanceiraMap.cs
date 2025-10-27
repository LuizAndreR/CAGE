using CakeGestao.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CakeGestao.Infrastructure.Data.Mappings;

internal class TransacaoFinanceiraMap : IEntityTypeConfiguration<TransacaoFinanceira>
{
    public void Configure(EntityTypeBuilder<TransacaoFinanceira> builder)
    {
        builder.ToTable("TransacoesFinanceiras");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Tipo)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Valor)
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        builder.Property(x => x.Data)
            .IsRequired();

        builder.Property(x => x.Descricao)
            .HasMaxLength(1000);

        builder.HasOne(x => x.Pedido)
            .WithMany(p => p.Transacoes)
            .HasForeignKey(t => t.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
