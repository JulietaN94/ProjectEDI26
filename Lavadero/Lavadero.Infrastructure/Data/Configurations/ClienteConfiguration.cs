using Lavadero.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lavadero.Infrastructure.Data.Configurations;

public sealed class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.NombreCompleto)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Telefono)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.FechaAlta)
            .HasColumnType("datetime2");

        builder.HasIndex(x => x.Telefono)
            .IsUnique();

        builder.HasMany(x => x.Autos)
            .WithOne(x => x.Cliente)
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
