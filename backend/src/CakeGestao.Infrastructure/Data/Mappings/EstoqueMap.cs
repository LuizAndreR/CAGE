using CakeGestao.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CakeGestao.Infrastructure.Data.Mappings;

public class EstoqueMap : IEntityTypeConfiguration<Estoque>
{
    public void Configure(EntityTypeBuilder<Estoque> builder)
    {
        builder.ToTable("Estoque");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Marca)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.QuantidadeAtual)
            .IsRequired()
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(0);
        
        builder.Property(x => x.QuantidadeMinina)
            .IsRequired()
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(3);
        
        builder.Property(x => x.UnidadeMedida)
            .IsRequired();
        
        builder.Property(x => x.ValorMedia)
            .IsRequired()
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(0);

        builder.Property(x => x.UnidadeReferenciaVolume)
            .IsRequired(false)
            .HasMaxLength(20);
        
        builder.Property(x => x.PesoReferenciaEmGramas)
            .HasPrecision(10, 2)
            .IsRequired(false);

        builder.HasOne(x => x.Empresa)
            .WithMany(x => x.ItemEstoques)
            .HasForeignKey(x => x.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
