using Lavadero.Application.DTOs;
using Lavadero.Application.Interfaces;
using Lavadero.Domain.Enums;
using Lavadero.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lavadero.Infrastructure.Services;

public sealed class DashboardAdminService
    : IDashboardAdminService
{
    private readonly IDbContextFactory<LavaderoDbContext> _factory;
    private readonly ILogger<DashboardAdminService> _logger;

    public DashboardAdminService(
        IDbContextFactory<LavaderoDbContext> factory,
        ILogger<DashboardAdminService> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public async Task<DashboardAdminDto> ObtenerResumenAsync()
    {
        try
        {
            await using var context =
                await _factory.CreateDbContextAsync();

            var hoy = DateTime.Today;
            var manana = hoy.AddDays(1);
            var ahora = DateTime.Now;

            var turnosHoy =
                context.Turnos
                    .AsNoTracking()
                    .Where(x =>
                        x.FechaHora >= hoy &&
                        x.FechaHora < manana);

            var totalHoy =
                await turnosHoy.CountAsync();

            var pendientes =
                await turnosHoy.CountAsync(
                    x => x.Estado == EstadoTurno.Pendiente);

            var confirmados =
                await turnosHoy.CountAsync(
                    x => x.Estado == EstadoTurno.Confirmado);

            var completados =
                await turnosHoy.CountAsync(
                    x => x.Estado == EstadoTurno.Completado);

            // Usar decimal nullable evita problemas de traducción
            // de DefaultIfEmpty(valor) en SQL Server.
            var ingresosHoy =
                await turnosHoy
                    .Where(
                        x => x.Estado == EstadoTurno.Completado)
                    .Select(x =>
                        (decimal?)(
                            x.PrecioServicio > 0
                                ? x.PrecioServicio
                                : x.Servicio.Precio))
                    .SumAsync()
                ?? 0m;

            var proximos =
                await context.Turnos
                    .AsNoTracking()
                    .Where(x =>
                        x.FechaHora >= ahora &&
                        (x.Estado == EstadoTurno.Pendiente ||
                         x.Estado == EstadoTurno.Confirmado))
                    .OrderBy(x => x.FechaHora)
                    .Take(5)
                    .Select(x => new TurnoDto(
                        x.Id,
                        x.FechaHora,
                        x.Estado,
                        x.Observaciones,
                        x.FechaCreacion,
                        x.Auto.Cliente.Id,
                        x.Auto.Cliente.NombreCompleto,
                        x.Auto.Cliente.Telefono,
                        x.Auto.Id,
                        x.Auto.Marca,
                        x.Auto.Modelo,
                        x.Servicio.Id,
                        x.Servicio.Nombre,
                        x.PrecioServicio > 0
                            ? x.PrecioServicio
                            : x.Servicio.Precio,
                        x.Servicio.DuracionMinutos))
                    .ToListAsync();

            return new DashboardAdminDto(
                totalHoy,
                pendientes,
                confirmados,
                completados,
                ingresosHoy,
                proximos);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "No se pudo obtener el resumen del dashboard.");

            throw;
        }
    }
}
