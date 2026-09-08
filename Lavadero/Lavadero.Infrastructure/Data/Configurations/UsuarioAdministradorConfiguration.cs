using Lavadero.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lavadero.Infrastructure.Data.Configurations;

public sealed class UsuarioAdministradorConfiguration : IEntityTypeConfiguration<UsuarioAdministrador>
{
    public void Configure(EntityTypeBuilder<UsuarioAdministrador> builder)
    {
        builder.ToTable("UsuariosAdministradores");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Usuario)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Activo)
            .HasDefaultValue(true);

        builder.HasIndex(x => x.Usuario)
            .IsUnique();
    }
}
