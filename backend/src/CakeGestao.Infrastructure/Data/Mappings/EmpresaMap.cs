using CakeGestao.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CakeGestao.Infrastructure.Data.Mappings;

public class EmpresaMap : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("Empresa");
        
        builder.Property(e => e.Nome)
            .HasMaxLength(150)
            .IsRequired();
        
        builder.Property(e => e.DataCadastro)
            .IsRequired()
            .HasDefaultValueSql("NOW()");
        
        builder.Property(e => e.Status)
            .HasMaxLength(150)
            .IsRequired();
    }
}