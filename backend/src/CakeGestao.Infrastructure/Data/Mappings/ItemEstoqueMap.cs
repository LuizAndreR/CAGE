using CakeGestao.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CakeGestao.Infrastructure.Data.Mappings;

internal class ItemEstoqueMap : IEntityTypeConfiguration<ItemEstoque>
{
    public void Configure(EntityTypeBuilder<ItemEstoque> builder)
    {
        builder.ToTable("ItensEstoque");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.QuantidadeAtual)
            .IsRequired()
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(0);

        builder.Property(x => x.UnidadeMedida)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.HasOne(x => x.Empresa)
            .WithMany(x => x.ItemEstoques)
            .HasForeignKey(x => x.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
