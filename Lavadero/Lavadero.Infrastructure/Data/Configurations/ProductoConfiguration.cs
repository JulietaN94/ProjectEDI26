using Lavadero.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lavadero.Infrastructure.Data.Configurations;

public sealed class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.HasIndex(x => x.Codigo).IsUnique();
        builder.Property(x => x.PrecioVenta).HasPrecision(18, 2);
    }
}
