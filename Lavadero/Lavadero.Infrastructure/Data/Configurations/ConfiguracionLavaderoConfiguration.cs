using Lavadero.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lavadero.Infrastructure.Data;

public sealed class ConfiguracionLavaderoConfiguration
    : IEntityTypeConfiguration<ConfiguracionLavadero>
{
    public void Configure(EntityTypeBuilder<ConfiguracionLavadero> builder)
    {
        builder.ToTable("ConfiguracionLavadero");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Telefono)
            .HasMaxLength(30);

        builder.Property(x => x.Direccion)
            .HasMaxLength(200);

        builder.Property(x => x.WhatsApp)
            .HasMaxLength(30);

        builder.Property(x => x.LogoContentType)
            .HasMaxLength(50);

        // El proveedor relacional elige el tipo de texto sin longitud.
        // En SQL Server se traduce a nvarchar(max) y permite ejecutar
        // pruebas de integración con SQLite sin acoplar el modelo.
        builder.Property(x => x.LogoBase64);
    }
}
