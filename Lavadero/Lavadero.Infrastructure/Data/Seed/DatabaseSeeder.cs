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
            },
            new Servicio
            {
                Id = -101,
                Nombre = "Lavado de motor",
                Descripcion = "Limpieza exterior del compartimiento del motor.",
                Precio = 16000m,
                DuracionMinutos = 45,
                Activo = true
            },
            new Servicio
            {
                Id = -102,
                Nombre = "Encerado manual",
                Descripcion = "Aplicación manual de cera protectora.",
                Precio = 22000m,
                DuracionMinutos = 60,
                Activo = true
            },
            new Servicio
            {
                Id = -103,
                Nombre = "Limpieza de tapizados",
                Descripcion = "Limpieza profunda de asientos y alfombras.",
                Precio = 32000m,
                DuracionMinutos = 120,
                Activo = true
            },
            new Servicio
            {
                Id = -104,
                Nombre = "Pulido de ópticas",
                Descripcion = "Recuperación y protección de ópticas delanteras.",
                Precio = 19000m,
                DuracionMinutos = 60,
                Activo = true
            },
            new Servicio
            {
                Id = -105,
                Nombre = "Tratamiento acrílico",
                Descripcion = "Protección acrílica para la pintura del vehículo.",
                Precio = 48000m,
                DuracionMinutos = 150,
                Activo = true
            },
            new Servicio
            {
                Id = -106,
                Nombre = "Descontaminado de pintura",
                Descripcion = "Remoción de contaminantes adheridos a la carrocería.",
                Precio = 28000m,
                DuracionMinutos = 90,
                Activo = true
            },
            new Servicio
            {
                Id = -107,
                Nombre = "Lavado de chasis",
                Descripcion = "Limpieza de bajos y chasis del vehículo.",
                Precio = 20000m,
                DuracionMinutos = 60,
                Activo = true
            });

        modelBuilder.Entity<Producto>().HasData(
            ProductoDemo(-1001, "DEMO-001", "Shampoo neutro 500 ml", "BrilloMax", 8500m),
            ProductoDemo(-1002, "DEMO-002", "Cera líquida 500 ml", "AutoCare", 12500m),
            ProductoDemo(-1003, "DEMO-003", "Limpiador de interiores", "CleanPro", 7800m),
            ProductoDemo(-1004, "DEMO-004", "Revividor de neumáticos", "DetailLab", 9900m),
            ProductoDemo(-1005, "DEMO-005", "Paño de microfibra", "Lavadero Test", 4200m),
            ProductoDemo(-1006, "DEMO-006", "Limpiavidrios 500 ml", "CleanPro", 6900m),
            ProductoDemo(-1007, "DEMO-007", "Cepillo para llantas", "AutoCare", 11300m),
            ProductoDemo(-1008, "DEMO-008", "Esponja de lavado", "BrilloMax", 3900m),
            ProductoDemo(-1009, "DEMO-009", "Sellador de pintura", "DetailLab", 18500m),
            ProductoDemo(-1010, "DEMO-010", "Aromatizante interior", "Lavadero Test", 5200m),
            ProductoDemo(-1011, "DEMO-011", "Kit de detailing básico", "AutoCare", 29500m),
            ProductoDemo(-1012, "DEMO-012", "Desengrasante multiuso", "CleanPro", 8900m));

        modelBuilder.Entity<HorarioAtencion>().HasData(
            new HorarioAtencion { Id = 1, DiaSemana = DayOfWeek.Monday, HoraApertura = new TimeSpan(8, 0, 0), HoraCierre = new TimeSpan(18, 0, 0), Activo = true },
            new HorarioAtencion { Id = 2, DiaSemana = DayOfWeek.Tuesday, HoraApertura = new TimeSpan(8, 0, 0), HoraCierre = new TimeSpan(18, 0, 0), Activo = true },
            new HorarioAtencion { Id = 3, DiaSemana = DayOfWeek.Wednesday, HoraApertura = new TimeSpan(8, 0, 0), HoraCierre = new TimeSpan(18, 0, 0), Activo = true },
            new HorarioAtencion { Id = 4, DiaSemana = DayOfWeek.Thursday, HoraApertura = new TimeSpan(8, 0, 0), HoraCierre = new TimeSpan(18, 0, 0), Activo = true },
            new HorarioAtencion { Id = 5, DiaSemana = DayOfWeek.Friday, HoraApertura = new TimeSpan(8, 0, 0), HoraCierre = new TimeSpan(18, 0, 0), Activo = true },
            new HorarioAtencion { Id = 6, DiaSemana = DayOfWeek.Saturday, HoraApertura = new TimeSpan(8, 0, 0), HoraCierre = new TimeSpan(14, 0, 0), Activo = true },
            new HorarioAtencion { Id = 7, DiaSemana = DayOfWeek.Sunday, HoraApertura = TimeSpan.Zero, HoraCierre = TimeSpan.Zero, Activo = false });

        modelBuilder.Entity<ConfiguracionLavadero>().HasData(
            new ConfiguracionLavadero
            {
                Id = 1,
                Nombre = "Mi Lavadero",
                Telefono = null,
                Direccion = null,
                WhatsApp = null
            });
    }

    private static Producto ProductoDemo(
        int id,
        string codigo,
        string nombre,
        string marca,
        decimal precio) =>
        new()
        {
            Id = id,
            Codigo = codigo,
            Nombre = nombre,
            Descripcion = "Producto de demostración incluido para presentar el catálogo y el flujo de pedidos.",
            Marca = marca,
            PrecioVenta = precio,
            Activo = true
        };
}
