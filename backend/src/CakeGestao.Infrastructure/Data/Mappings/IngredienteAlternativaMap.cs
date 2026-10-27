using CakeGestao.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CakeGestao.Infrastructure.Data.Mappings;

internal class IngredienteAlternativaMap : IEntityTypeConfiguration<IngredienteAlternativa>
{
    public void Configure(EntityTypeBuilder<IngredienteAlternativa> builder)
    {
        builder.ToTable("IngredienteAlternativas");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.IngredienteId, x.ItemId }).IsUnique();
        builder.HasOne<Ingrediente>().WithMany(x => x.Alternativas)
            .HasForeignKey(x => x.IngredienteId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Item).WithMany()
            .HasForeignKey(x => x.ItemId).OnDelete(DeleteBehavior.Restrict);
    }
}
