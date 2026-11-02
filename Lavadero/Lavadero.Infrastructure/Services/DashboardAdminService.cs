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
    private readonly IAppClock _clock;

    public DashboardAdminService(
        IDbContextFactory<LavaderoDbContext> factory,
        ILogger<DashboardAdminService> logger,
        IAppClock clock)
    {
        _factory = factory;
        _logger = logger;
        _clock = clock;
    }

    public async Task<DashboardAdminDto> ObtenerResumenAsync()
    {
        try
        {
            await using var context =
                await _factory.CreateDbContextAsync();

            var hoy = _clock.LocalToday;
            var manana = hoy.AddDays(1);
            var ahora = _clock.LocalNow;

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
                            x.PrecioServicio))
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
                        x.PrecioServicio,
                        x.Servicio.DuracionMinutos))
                    .ToListAsync();

            var inicioDiaUtc = _clock.ToUtc(hoy);
            var finDiaUtc = _clock.ToUtc(manana);

            var pedidosHoy = await context.Pedidos
                .AsNoTracking()
                .CountAsync(x => x.FechaUtc >= inicioDiaUtc && x.FechaUtc < finDiaUtc);

            var pedidosPendientes = await context.Pedidos
                .AsNoTracking()
                .CountAsync(x => x.Estado == Domain.Entities.EstadoPedido.Pendiente);

            var pedidosRecientes = await context.Pedidos
                .AsNoTracking()
                .OrderByDescending(x => x.Id)
                .Take(3)
                .Select(x => new PedidoDto(
                    x.Id,
                    x.Nombre,
                    x.Apellido,
                    x.Celular,
                    x.FechaUtc,
                    x.Estado,
                    x.Detalles
                        .Select(d => new DetallePedidoDto(
                            d.Codigo,
                            d.Nombre,
                            d.PrecioUnitario,
                            d.Cantidad))
                        .ToList()))
                .ToListAsync();

            return new DashboardAdminDto(
                totalHoy,
                pendientes,
                confirmados,
                completados,
                ingresosHoy,
                proximos,
                pedidosHoy,
                pedidosPendientes,
                pedidosRecientes);
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
