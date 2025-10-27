using CakeGestao.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CakeGestao.Infrastructure.Data.Mappings;

internal class IngredienteMap : IEntityTypeConfiguration<Ingrediente>
{
    public void Configure(EntityTypeBuilder<Ingrediente> builder)
    {
        builder.ToTable("Ingredientes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantidade)
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        builder.Property(x => x.UnidadeMedida)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne(x => x.Receita)
            .WithMany(r => r.Ingredientes)
            .HasForeignKey(x => x.ReceitaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Item)
            .WithMany(i => i.Ingredientes)
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
