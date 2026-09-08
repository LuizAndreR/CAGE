using CakeGestao.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CakeGestao.Infrastructure.Data.Mappings;

public class TokenRefreshMap : IEntityTypeConfiguration<TokenRefresh>
{
    public void Configure(EntityTypeBuilder<TokenRefresh> builder)
    {
        builder.ToTable("TokenRefresh");

        builder.HasKey(k => k.Id);
        builder.Property(p => p.Id)
               .ValueGeneratedOnAdd()
               .HasColumnName("Id");

        builder.Property(p => p.RefreshToken)
               .IsRequired()
               .HasMaxLength(500)
               .HasColumnName("RefreshToken");

        builder.HasIndex(p => p.RefreshToken)
               .IsUnique();

        builder.Property(p => p.CreatedAt)
               .IsRequired()
               .HasColumnType("datetime2")
               .HasDefaultValueSql("GETUTCDATE()")
               .HasColumnName("CreatedAt");

        builder.Property(p => p.ExpiresAt)
               .IsRequired()
               .HasColumnType("datetime2")
               .HasColumnName("ExpiresAt");

        builder.Property(p => p.IsRevoked)
               .IsRequired()
               .HasDefaultValue(false)
               .HasColumnName("IsRevoked");

        builder.Property(p => p.IsUsed)
               .IsRequired()
               .HasDefaultValue(false)
               .HasColumnName("IsUsed");

        builder.HasOne(p => p.Usuario)
               .WithMany()
               .HasForeignKey(p => p.UsuarioId)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();
    }
}
