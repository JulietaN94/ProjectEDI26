using Lavadero.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lavadero.Infrastructure.Data.Configurations;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(x => x.NombreCompleto)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Activo)
            .HasDefaultValue(true);
    }
}
