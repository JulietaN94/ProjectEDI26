using Lavadero.Application.Common;
using Lavadero.Application.DTOs;
using Lavadero.Application.Interfaces;
using Lavadero.Application.Models;
using Lavadero.Domain.Entities;
using Lavadero.Domain.Enums;
using Lavadero.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lavadero.Infrastructure.Services;

public sealed class TurnoService : ITurnoService
{
    private readonly IDbContextFactory<LavaderoDbContext> _factory;

    public TurnoService(IDbContextFactory<LavaderoDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<TurnoDto> CrearReservaAsync(CrearReservaRequest request)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var servicio = await context.Servicios
            .FirstOrDefaultAsync(x => x.Id == request.ServicioId && x.Activo)
            ?? throw new InvalidOperationException("El servicio seleccionado no está disponible.");

        await ValidarHorarioYDisponibilidadAsync(
            context,
            request.FechaHora,
            servicio.DuracionMinutos);

        var telefono = request.Telefono.Trim();
        var nombreCompleto = request.NombreCompleto.Trim();
        var marca = request.Marca.Trim();
        var modelo = request.Modelo.Trim();

        var cliente = await context.Clientes
            .Include(x => x.Autos)
            .FirstOrDefaultAsync(x => x.Telefono == telefono);

        if (cliente is null)
        {
            cliente = new Cliente
            {
                NombreCompleto = nombreCompleto,
                Telefono = telefono,
                FechaAlta = DateTime.UtcNow
            };

            context.Clientes.Add(cliente);
        }
        else if (!string.Equals(cliente.NombreCompleto, nombreCompleto, StringComparison.Ordinal))
        {
            cliente.NombreCompleto = nombreCompleto;
        }

        var auto = cliente.Autos.FirstOrDefault(x =>
            string.Equals(x.Marca, marca, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.Modelo, modelo, StringComparison.OrdinalIgnoreCase));

        if (auto is null)
        {
            auto = new Auto
            {
                Marca = marca,
                Modelo = modelo,
                Cliente = cliente
            };

            cliente.Autos.Add(auto);
        }

        var turno = new Turno
        {
            Auto = auto,
            Servicio = servicio,
            FechaHora = request.FechaHora,
            Estado = EstadoTurno.Pendiente,
            Observaciones = NormalizarOpcional(request.Observaciones),
            FechaCreacion = DateTime.UtcNow
        };

        context.Turnos.Add(turno);
        await context.SaveChangesAsync();

        return MapearDtoMemoria(turno, cliente, auto, servicio);
    }

    public async Task<TurnoDto?> ObtenerPorIdAsync(int id)
    {
        await using var context = await _factory.CreateDbContextAsync();

        return await QueryTurnos(context)
            .Where(x => x.Id == id)
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
                x.Servicio.Precio,
                x.Servicio.DuracionMinutos))
            .FirstOrDefaultAsync();
    }

    public async Task<PagedResult<TurnoDto>> ObtenerPaginadosAsync(
        string? busqueda,
        EstadoTurno? estado,
        int pagina,
        int tamanioPagina)
    {
        ValidarPaginacion(ref pagina, ref tamanioPagina);

        await using var context = await _factory.CreateDbContextAsync();

        var query = QueryTurnos(context);

        if (estado.HasValue)
            query = query.Where(x => x.Estado == estado.Value);

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var patron = $"%{busqueda.Trim()}%";

            query = query.Where(x =>
                EF.Functions.Like(x.Auto.Cliente.NombreCompleto, patron) ||
                EF.Functions.Like(x.Auto.Cliente.Telefono, patron) ||
                EF.Functions.Like(x.Auto.Marca, patron) ||
                EF.Functions.Like(x.Auto.Modelo, patron) ||
                EF.Functions.Like(x.Servicio.Nombre, patron));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.FechaHora)
            .Skip((pagina - 1) * tamanioPagina)
            .Take(tamanioPagina)
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
                x.Servicio.Precio,
                x.Servicio.DuracionMinutos))
            .ToListAsync();

        return new PagedResult<TurnoDto>
        {
            Items = items,
            Page = pagina,
            PageSize = tamanioPagina,
            TotalItems = total
        };
    }

    public async Task CambiarEstadoAsync(int turnoId, EstadoTurno nuevoEstado)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var turno = await context.Turnos.FindAsync(turnoId)
            ?? throw new KeyNotFoundException("No se encontró el turno indicado.");

        if (!EsTransicionValida(turno.Estado, nuevoEstado))
        {
            throw new InvalidOperationException(
                $"No se puede cambiar el turno de {turno.Estado} a {nuevoEstado}.");
        }

        turno.Estado = nuevoEstado;
        await context.SaveChangesAsync();
    }

    public Task CancelarAsync(int turnoId) =>
        CambiarEstadoAsync(turnoId, EstadoTurno.Cancelado);

    private static IQueryable<Turno> QueryTurnos(LavaderoDbContext context) =>
        context.Turnos
            .AsNoTracking()
            .Include(x => x.Auto)
                .ThenInclude(x => x.Cliente)
            .Include(x => x.Servicio);

    private static TurnoDto MapearDtoMemoria(
        Turno turno,
        Cliente cliente,
        Auto auto,
        Servicio servicio) => new(
            turno.Id,
            turno.FechaHora,
            turno.Estado,
            turno.Observaciones,
            turno.FechaCreacion,
            cliente.Id,
            cliente.NombreCompleto,
            cliente.Telefono,
            auto.Id,
            auto.Marca,
            auto.Modelo,
            servicio.Id,
            servicio.Nombre,
            servicio.Precio,
            servicio.DuracionMinutos);

    private static async Task ValidarHorarioYDisponibilidadAsync(
        LavaderoDbContext context,
        DateTime fechaHora,
        int duracionMinutos)
    {
        if (fechaHora <= DateTime.Now)
            throw new InvalidOperationException("La fecha y hora del turno debe ser futura.");

        var horario = await context.HorariosAtencion
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.DiaSemana == fechaHora.DayOfWeek && x.Activo);

        if (horario is null)
            throw new InvalidOperationException("El lavadero no atiende el día seleccionado.");

        var horaInicio = fechaHora.TimeOfDay;
        var horaFin = horaInicio.Add(TimeSpan.FromMinutes(duracionMinutos));

        if (horaInicio < horario.HoraApertura || horaFin > horario.HoraCierre)
        {
            throw new InvalidOperationException(
                "El turno seleccionado queda fuera del horario de atención.");
        }

        var inicioDia = fechaHora.Date;
        var finDia = inicioDia.AddDays(1);

        var turnosDelDia = await context.Turnos
            .AsNoTracking()
            .Include(x => x.Servicio)
            .Where(x =>
                x.FechaHora >= inicioDia &&
                x.FechaHora < finDia &&
                x.Estado != EstadoTurno.Cancelado)
            .ToListAsync();

        var fechaHoraFin = fechaHora.AddMinutes(duracionMinutos);

        var existeSuperposicion = turnosDelDia.Any(x =>
        {
            var finExistente = x.FechaHora.AddMinutes(x.Servicio.DuracionMinutos);
            return x.FechaHora < fechaHoraFin && finExistente > fechaHora;
        });

        if (existeSuperposicion)
            throw new InvalidOperationException("El horario seleccionado ya no está disponible.");
    }

    private static bool EsTransicionValida(EstadoTurno actual, EstadoTurno nuevo)
    {
        if (actual == nuevo)
            return true;

        return actual switch
        {
            EstadoTurno.Pendiente => nuevo is EstadoTurno.Confirmado or EstadoTurno.Cancelado,
            EstadoTurno.Confirmado => nuevo is EstadoTurno.Completado or EstadoTurno.Cancelado,
            EstadoTurno.Completado => false,
            EstadoTurno.Cancelado => false,
            _ => false
        };
    }

    private static string? NormalizarOpcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static void ValidarPaginacion(ref int pagina, ref int tamanioPagina)
    {
        if (pagina < 1)
            pagina = 1;

        if (tamanioPagina < 1)
            tamanioPagina = 10;

        if (tamanioPagina > 100)
            tamanioPagina = 100;
    }
}
