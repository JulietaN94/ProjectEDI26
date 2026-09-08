using Lavadero.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lavadero.Infrastructure.Data.Configurations;

public sealed class AutoConfiguration : IEntityTypeConfiguration<Auto>
{
    public void Configure(EntityTypeBuilder<Auto> builder)
    {
        builder.ToTable("Autos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Marca)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Modelo)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.ClienteId);
    }
}
