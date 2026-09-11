using Lavadero.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lavadero.Infrastructure.Data.Configurations;

public sealed class TurnoConfiguration : IEntityTypeConfiguration<Turno>
{
    public void Configure(EntityTypeBuilder<Turno> builder)
    {
        builder.ToTable("Turnos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FechaHora)
            .HasColumnType("datetime2");

        builder.Property(x => x.FechaCreacion)
            .HasColumnType("datetime2");

        builder.Property(x => x.Estado)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.PrecioServicio)
            .HasPrecision(18, 2);

        builder.Property(x => x.Observaciones)
            .HasMaxLength(500);

        builder.Property(x => x.TokenConfirmacion)
            .HasMaxLength(64);

        builder.Property(x => x.FechaExpiracionConfirmacion)
            .HasColumnType("datetime2");

        builder.Property(x => x.FechaConfirmacion)
            .HasColumnType("datetime2");

        builder.Property(x => x.TokenCancelacion)
            .HasMaxLength(64);

        builder.HasIndex(x => x.FechaHora);
        builder.HasIndex(x => x.Estado);

        // Solo los turnos que tengan token participan del índice único.
        // Esto permite que los turnos anteriores sigan teniendo NULL.
        builder.HasIndex(x => x.TokenConfirmacion)
            .IsUnique()
            .HasFilter("[TokenConfirmacion] IS NOT NULL");

        builder.HasIndex(x => x.TokenCancelacion)
            .IsUnique()
            .HasFilter("[TokenCancelacion] IS NOT NULL");

        builder.HasOne(x => x.Auto)
            .WithMany(x => x.Turnos)
            .HasForeignKey(x => x.AutoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Servicio)
            .WithMany(x => x.Turnos)
            .HasForeignKey(x => x.ServicioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
