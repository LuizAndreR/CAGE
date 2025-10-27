using CakeGestao.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CakeGestao.Infrastructure.Data.Mappings;

internal class ReceitaMap : IEntityTypeConfiguration<Receita>
{
    public void Configure(EntityTypeBuilder<Receita> builder)
    {
        builder.ToTable("Receitas");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.ModoPreparo)
            .HasMaxLength(4000);

        builder.Property(x => x.PrecoVenda)
            .IsRequired()
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(0);

    }
}
