using Lavadero.Application.Common;
using Lavadero.Application.DTOs;
using Lavadero.Application.Interfaces;
using Lavadero.Application.Models;
using Lavadero.Domain.Entities;
using Lavadero.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lavadero.Infrastructure.Services;

public sealed class ServicioService : IServicioService
{
    private readonly IDbContextFactory<LavaderoDbContext> _factory;
    private readonly ILogger<ServicioService> _logger;

    public ServicioService(
        IDbContextFactory<LavaderoDbContext> factory,
        ILogger<ServicioService> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ServicioDto>> ObtenerActivosAsync()
    {
        await using var context =
            await _factory.CreateDbContextAsync();

        return await context.Servicios
            .AsNoTracking()
            .Where(x => x.Activo)
            .OrderBy(x => x.Nombre)
            .Select(x => new ServicioDto(
                x.Id,
                x.Nombre,
                x.Descripcion,
                x.Precio,
                x.DuracionMinutos,
                x.Activo))
            .ToListAsync();
    }

    public async Task<ServicioDto?> ObtenerPorIdAsync(int id)
    {
        await using var context =
            await _factory.CreateDbContextAsync();

        return await context.Servicios
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ServicioDto(
                x.Id,
                x.Nombre,
                x.Descripcion,
                x.Precio,
                x.DuracionMinutos,
                x.Activo))
            .FirstOrDefaultAsync();
    }

    public async Task<PagedResult<ServicioDto>> ObtenerPaginadosAsync(
        string? busqueda,
        bool? activo,
        int pagina,
        int tamanioPagina)
    {
        ValidarPaginacion(
            ref pagina,
            ref tamanioPagina);

        await using var context =
            await _factory.CreateDbContextAsync();

        var query =
            context.Servicios
                .AsNoTracking()
                .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var patron =
                $"%{busqueda.Trim()}%";

            query = query.Where(x =>
                EF.Functions.Like(
                    x.Nombre,
                    patron) ||
                (x.Descripcion != null &&
                 EF.Functions.Like(
                     x.Descripcion,
                     patron)));
        }

        if (activo.HasValue)
        {
            query =
                query.Where(
                    x => x.Activo == activo.Value);
        }

        var total =
            await query.CountAsync();

        var items = await query
            .OrderBy(x => x.Nombre)
            .Skip((pagina - 1) * tamanioPagina)
            .Take(tamanioPagina)
            .Select(x => new ServicioDto(
                x.Id,
                x.Nombre,
                x.Descripcion,
                x.Precio,
                x.DuracionMinutos,
                x.Activo))
            .ToListAsync();

        return new PagedResult<ServicioDto>
        {
            Items = items,
            Page = pagina,
            PageSize = tamanioPagina,
            TotalItems = total
        };
    }

    public async Task<ServicioDto> CrearAsync(
        GuardarServicioRequest request)
    {
        await using var context =
            await _factory.CreateDbContextAsync();

        var nombre =
            request.Nombre.Trim();

        if (await context.Servicios.AnyAsync(
                x => x.Nombre == nombre))
        {
            _logger.LogWarning(
                "Se intentó crear un servicio duplicado: {Nombre}.",
                nombre);

            throw new InvalidOperationException(
                "Ya existe un servicio con ese nombre.");
        }

        var servicio = new Servicio
        {
            Nombre = nombre,
            Descripcion =
                NormalizarOpcional(
                    request.Descripcion),
            Precio = request.Precio,
            DuracionMinutos =
                request.DuracionMinutos,
            Activo = request.Activo
        };

        context.Servicios.Add(servicio);

        await context.SaveChangesAsync();

        _logger.LogInformation(
            "Servicio {ServicioId} creado: {Nombre}.",
            servicio.Id,
            servicio.Nombre);

        return MapearDtoMemoria(servicio);
    }

    public async Task<ServicioDto> ActualizarAsync(
        int id,
        GuardarServicioRequest request)
    {
        await using var context =
            await _factory.CreateDbContextAsync();

        var servicio =
            await context.Servicios.FindAsync(id)
            ?? throw new KeyNotFoundException(
                "No se encontró el servicio indicado.");

        var nombre =
            request.Nombre.Trim();

        if (await context.Servicios.AnyAsync(
                x => x.Id != id &&
                     x.Nombre == nombre))
        {
            _logger.LogWarning(
                "No se pudo actualizar el servicio {ServicioId}: nombre duplicado {Nombre}.",
                id,
                nombre);

            throw new InvalidOperationException(
                "Ya existe otro servicio con ese nombre.");
        }

        servicio.Nombre = nombre;
        servicio.Descripcion =
            NormalizarOpcional(
                request.Descripcion);
        servicio.Precio = request.Precio;
        servicio.DuracionMinutos =
            request.DuracionMinutos;
        servicio.Activo = request.Activo;

        await context.SaveChangesAsync();

        _logger.LogInformation(
            "Servicio {ServicioId} actualizado.",
            id);

        return MapearDtoMemoria(servicio);
    }

    public async Task EliminarAsync(int id)
    {
        await using var context =
            await _factory.CreateDbContextAsync();

        var servicio =
            await context.Servicios.FindAsync(id)
            ?? throw new KeyNotFoundException(
                "No se encontró el servicio indicado.");

        var tieneTurnos =
            await context.Turnos.AnyAsync(
                x => x.ServicioId == id);

        if (tieneTurnos)
        {
            _logger.LogWarning(
                "No se pudo eliminar el servicio {ServicioId} porque tiene turnos asociados.",
                id);

            throw new InvalidOperationException(
                "El servicio tiene turnos asociados. " +
                "Desactívelo para conservar el historial.");
        }

        context.Servicios.Remove(servicio);

        await context.SaveChangesAsync();

        _logger.LogInformation(
            "Servicio {ServicioId} eliminado.",
            id);
    }

    private static ServicioDto MapearDtoMemoria(
        Servicio servicio) =>
        new(
            servicio.Id,
            servicio.Nombre,
            servicio.Descripcion,
            servicio.Precio,
            servicio.DuracionMinutos,
            servicio.Activo);

    private static string? NormalizarOpcional(
        string? valor) =>
        string.IsNullOrWhiteSpace(valor)
            ? null
            : valor.Trim();

    private static void ValidarPaginacion(
        ref int pagina,
        ref int tamanioPagina)
    {
        if (pagina < 1)
            pagina = 1;

        if (tamanioPagina < 1)
            tamanioPagina = 10;

        if (tamanioPagina > 100)
            tamanioPagina = 100;
    }
}
