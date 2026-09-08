using Lavadero.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lavadero.Infrastructure.Data.Seed;

internal static class DatabaseSeeder
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Servicio>().HasData(
            new Servicio
            {
                Id = 1,
                Nombre = "Lavado exterior",
                Descripcion = "Lavado exterior del vehículo.",
                Precio = 10000m,
                DuracionMinutos = 30,
                Activo = true
            },
            new Servicio
            {
                Id = 2,
                Nombre = "Lavado completo",
                Descripcion = "Lavado exterior e interior del vehículo.",
                Precio = 18000m,
                DuracionMinutos = 60,
                Activo = true
            },
            new Servicio
            {
                Id = 3,
                Nombre = "Lavado premium",
                Descripcion = "Lavado completo con terminación premium.",
                Precio = 25000m,
                DuracionMinutos = 90,
                Activo = true
            });

        modelBuilder.Entity<HorarioAtencion>().HasData(
            new HorarioAtencion { Id = 1, DiaSemana = DayOfWeek.Monday, HoraApertura = new TimeSpan(8, 0, 0), HoraCierre = new TimeSpan(18, 0, 0), Activo = true },
            new HorarioAtencion { Id = 2, DiaSemana = DayOfWeek.Tuesday, HoraApertura = new TimeSpan(8, 0, 0), HoraCierre = new TimeSpan(18, 0, 0), Activo = true },
            new HorarioAtencion { Id = 3, DiaSemana = DayOfWeek.Wednesday, HoraApertura = new TimeSpan(8, 0, 0), HoraCierre = new TimeSpan(18, 0, 0), Activo = true },
            new HorarioAtencion { Id = 4, DiaSemana = DayOfWeek.Thursday, HoraApertura = new TimeSpan(8, 0, 0), HoraCierre = new TimeSpan(18, 0, 0), Activo = true },
            new HorarioAtencion { Id = 5, DiaSemana = DayOfWeek.Friday, HoraApertura = new TimeSpan(8, 0, 0), HoraCierre = new TimeSpan(18, 0, 0), Activo = true },
            new HorarioAtencion { Id = 6, DiaSemana = DayOfWeek.Saturday, HoraApertura = new TimeSpan(8, 0, 0), HoraCierre = new TimeSpan(14, 0, 0), Activo = true },
            new HorarioAtencion { Id = 7, DiaSemana = DayOfWeek.Sunday, HoraApertura = TimeSpan.Zero, HoraCierre = TimeSpan.Zero, Activo = false });
    }
}
