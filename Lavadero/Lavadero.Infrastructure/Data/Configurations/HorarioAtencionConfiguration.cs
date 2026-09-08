using Lavadero.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lavadero.Infrastructure.Data.Configurations;

public sealed class HorarioAtencionConfiguration : IEntityTypeConfiguration<HorarioAtencion>
{
    public void Configure(EntityTypeBuilder<HorarioAtencion> builder)
    {
        builder.ToTable("HorariosAtencion");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DiaSemana)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.HoraApertura)
            .HasColumnType("time");

        builder.Property(x => x.HoraCierre)
            .HasColumnType("time");

        builder.Property(x => x.Activo)
            .HasDefaultValue(true);

        builder.HasIndex(x => x.DiaSemana)
            .IsUnique();
    }
}
