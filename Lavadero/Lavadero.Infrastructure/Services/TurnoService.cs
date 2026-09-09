using Lavadero.Application.Common;
using Lavadero.Application.DTOs;
using Lavadero.Application.Interfaces;
using Lavadero.Application.Models;
using Lavadero.Domain.Entities;
using Lavadero.Domain.Enums;
using Lavadero.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lavadero.Infrastructure.Services;

public sealed class TurnoService : ITurnoService
{
    private const int PasoMinutos = 30;

    private readonly IDbContextFactory<LavaderoDbContext> _factory;
    private readonly ILogger<TurnoService> _logger;

    public TurnoService(
        IDbContextFactory<LavaderoDbContext> factory,
        ILogger<TurnoService> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public async Task<TurnoDto> CrearReservaAsync(
        CrearReservaRequest request)
    {
        ValidarVentanaReserva(request.FechaHora);

        await using var context =
            await _factory.CreateDbContextAsync();

        var servicio =
            await context.Servicios
                .FirstOrDefaultAsync(
                    x => x.Id == request.ServicioId &&
                         x.Activo)
            ?? throw new InvalidOperationException(
                "El servicio seleccionado no está disponible.");

        await ValidarHorarioYDisponibilidadAsync(
            context,
            request.FechaHora,
            servicio.DuracionMinutos);

        var telefono = request.Telefono.Trim();
        var nombreCompleto = request.NombreCompleto.Trim();
        var marca = request.Marca.Trim();
        var modelo = request.Modelo.Trim();

        var cliente =
            await context.Clientes
                .Include(x => x.Autos)
                .FirstOrDefaultAsync(
                    x => x.Telefono == telefono);

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
        else if (!string.Equals(
                     cliente.NombreCompleto,
                     nombreCompleto,
                     StringComparison.Ordinal))
        {
            cliente.NombreCompleto = nombreCompleto;
        }

        var auto =
            cliente.Autos.FirstOrDefault(x =>
                string.Equals(
                    x.Marca,
                    marca,
                    StringComparison.OrdinalIgnoreCase) &&
                string.Equals(
                    x.Modelo,
                    modelo,
                    StringComparison.OrdinalIgnoreCase));

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
            PrecioServicio = servicio.Precio,
            Observaciones =
                NormalizarOpcional(
                    request.Observaciones),
            FechaCreacion = DateTime.UtcNow
        };

        context.Turnos.Add(turno);

        await context.SaveChangesAsync();

        _logger.LogInformation(
            "Turno {TurnoId} creado para servicio {ServicioId} en {FechaHora}.",
            turno.Id,
            servicio.Id,
            turno.FechaHora);

        return MapearDtoMemoria(
            turno,
            cliente,
            auto,
            servicio);
    }

    public async Task<IReadOnlyList<TimeSpan>>
        ObtenerHorariosDisponiblesAsync(
            int servicioId,
            DateTime fecha)
    {
        var dia = fecha.Date;
        var hoy = DateTime.Today;

        // Solo se permiten fechas desde hoy hasta el último día
        // del mes actual.
        if (dia < hoy ||
            dia.Year != hoy.Year ||
            dia.Month != hoy.Month)
        {
            return Array.Empty<TimeSpan>();
        }

        await using var context =
            await _factory.CreateDbContextAsync();

        var servicio =
            await context.Servicios
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == servicioId &&
                         x.Activo);

        if (servicio is null)
        {
            return Array.Empty<TimeSpan>();
        }

        var horario =
            await context.HorariosAtencion
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.DiaSemana == dia.DayOfWeek &&
                    x.Activo);

        if (horario is null)
        {
            return Array.Empty<TimeSpan>();
        }

        var finDia = dia.AddDays(1);

        var turnosDelDia =
            await context.Turnos
                .AsNoTracking()
                .Include(x => x.Servicio)
                .Where(x =>
                    x.FechaHora >= dia &&
                    x.FechaHora < finDia &&
                    x.Estado != EstadoTurno.Cancelado)
                .ToListAsync();

        var disponibles = new List<TimeSpan>();
        var duracion =
            TimeSpan.FromMinutes(
                servicio.DuracionMinutos);

        var paso =
            TimeSpan.FromMinutes(PasoMinutos);

        for (var hora = horario.HoraApertura;
             hora + duracion <= horario.HoraCierre;
             hora += paso)
        {
            var inicio = dia.Add(hora);
            var fin = inicio.Add(duracion);

            if (inicio <= DateTime.Now)
            {
                continue;
            }

            var ocupado =
                turnosDelDia.Any(turno =>
                {
                    var inicioExistente =
                        turno.FechaHora;

                    var finExistente =
                        turno.FechaHora.AddMinutes(
                            turno.Servicio.DuracionMinutos);

                    return inicio < finExistente &&
                           fin > inicioExistente;
                });

            if (!ocupado)
            {
                disponibles.Add(hora);
            }
        }

        return disponibles;
    }

    public async Task<TurnoDto?> ObtenerPorIdAsync(int id)
    {
        await using var context =
            await _factory.CreateDbContextAsync();

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
                x.PrecioServicio > 0
                    ? x.PrecioServicio
                    : x.Servicio.Precio,
                x.Servicio.DuracionMinutos))
            .FirstOrDefaultAsync();
    }

    public async Task<PagedResult<TurnoDto>>
        ObtenerPaginadosAsync(
            string? busqueda,
            EstadoTurno? estado,
            int pagina,
            int tamanioPagina)
    {
        ValidarPaginacion(
            ref pagina,
            ref tamanioPagina);

        await using var context =
            await _factory.CreateDbContextAsync();

        var query =
            AplicarFiltros(
                QueryTurnos(context),
                busqueda,
                estado);

        return await PaginarAsync(
            query,
            pagina,
            tamanioPagina,
            ordenarAscendente: false);
    }

    public async Task<PagedResult<TurnoDto>>
        ObtenerHistorialAsync(
            string? busqueda,
            DateTime? desde,
            DateTime? hasta,
            EstadoTurno? estado,
            int pagina,
            int tamanioPagina)
    {
        ValidarPaginacion(
            ref pagina,
            ref tamanioPagina);

        await using var context =
            await _factory.CreateDbContextAsync();

        var ahora = DateTime.Now;

        var query =
            QueryTurnos(context)
                .Where(x =>
                    x.FechaHora < ahora ||
                    x.Estado == EstadoTurno.Completado ||
                    x.Estado == EstadoTurno.Cancelado);

        query =
            AplicarFiltros(
                query,
                busqueda,
                estado);

        if (desde.HasValue)
        {
            var fechaDesde =
                desde.Value.Date;

            query =
                query.Where(
                    x => x.FechaHora >= fechaDesde);
        }

        if (hasta.HasValue)
        {
            var fechaHastaExclusiva =
                hasta.Value.Date.AddDays(1);

            query =
                query.Where(
                    x => x.FechaHora < fechaHastaExclusiva);
        }

        return await PaginarAsync(
            query,
            pagina,
            tamanioPagina,
            ordenarAscendente: false);
    }

    public async Task<DateTime?> ObtenerPrimeraFechaRegistroAsync()
    {
        await using var context =
            await _factory.CreateDbContextAsync();

        return await context.Turnos
            .AsNoTracking()
            .MinAsync(x =>
                (DateTime?)x.FechaCreacion);
    }

    public async Task CambiarEstadoAsync(
        int turnoId,
        EstadoTurno nuevoEstado)
    {
        await using var context =
            await _factory.CreateDbContextAsync();

        var turno =
            await context.Turnos.FindAsync(turnoId)
            ?? throw new KeyNotFoundException(
                "No se encontró el turno indicado.");

        var estadoAnterior = turno.Estado;

        if (!EsTransicionValida(
                turno.Estado,
                nuevoEstado))
        {
            throw new InvalidOperationException(
                $"No se puede cambiar el turno de {turno.Estado} a {nuevoEstado}.");
        }

        turno.Estado = nuevoEstado;

        await context.SaveChangesAsync();

        _logger.LogInformation(
            "Turno {TurnoId} cambió de {EstadoAnterior} a {EstadoNuevo}.",
            turnoId,
            estadoAnterior,
            nuevoEstado);
    }

    public Task CancelarAsync(int turnoId) =>
        CambiarEstadoAsync(
            turnoId,
            EstadoTurno.Cancelado);

    private static void ValidarVentanaReserva(
        DateTime fechaHora)
    {
        var ahora = DateTime.Now;
        var hoy = ahora.Date;

        if (fechaHora <= ahora)
        {
            throw new InvalidOperationException(
                "La fecha y hora del turno debe ser futura.");
        }

        if (fechaHora.Date < hoy)
        {
            throw new InvalidOperationException(
                "No se pueden reservar turnos en días anteriores.");
        }

        if (fechaHora.Year != hoy.Year ||
            fechaHora.Month != hoy.Month)
        {
            throw new InvalidOperationException(
                "Los turnos solo pueden reservarse dentro del mes actual.");
        }
    }

    private static IQueryable<Turno> QueryTurnos(
        LavaderoDbContext context) =>
        context.Turnos
            .AsNoTracking()
            .Include(x => x.Auto)
                .ThenInclude(x => x.Cliente)
            .Include(x => x.Servicio);

    private static IQueryable<Turno> AplicarFiltros(
        IQueryable<Turno> query,
        string? busqueda,
        EstadoTurno? estado)
    {
        if (estado.HasValue)
        {
            query =
                query.Where(
                    x => x.Estado == estado.Value);
        }

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var patron =
                $"%{busqueda.Trim()}%";

            query = query.Where(x =>
                EF.Functions.Like(
                    x.Auto.Cliente.NombreCompleto,
                    patron) ||
                EF.Functions.Like(
                    x.Auto.Cliente.Telefono,
                    patron) ||
                EF.Functions.Like(
                    x.Auto.Marca,
                    patron) ||
                EF.Functions.Like(
                    x.Auto.Modelo,
                    patron) ||
                EF.Functions.Like(
                    x.Servicio.Nombre,
                    patron));
        }

        return query;
    }

    private static async Task<PagedResult<TurnoDto>>
        PaginarAsync(
            IQueryable<Turno> query,
            int pagina,
            int tamanioPagina,
            bool ordenarAscendente)
    {
        var total =
            await query.CountAsync();

        var ordenada =
            ordenarAscendente
                ? query.OrderBy(x => x.FechaHora)
                : query.OrderByDescending(
                    x => x.FechaHora);

        var items =
            await ordenada
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
                    x.PrecioServicio > 0
                        ? x.PrecioServicio
                        : x.Servicio.Precio,
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

    private static TurnoDto MapearDtoMemoria(
        Turno turno,
        Cliente cliente,
        Auto auto,
        Servicio servicio) =>
        new(
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
            turno.PrecioServicio,
            servicio.DuracionMinutos);

    private static async Task
        ValidarHorarioYDisponibilidadAsync(
            LavaderoDbContext context,
            DateTime fechaHora,
            int duracionMinutos)
    {
        ValidarVentanaReserva(fechaHora);

        var horario =
            await context.HorariosAtencion
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.DiaSemana == fechaHora.DayOfWeek &&
                    x.Activo);

        if (horario is null)
        {
            throw new InvalidOperationException(
                "El lavadero no atiende el día seleccionado.");
        }

        var horaInicio =
            fechaHora.TimeOfDay;

        var horaFin =
            horaInicio.Add(
                TimeSpan.FromMinutes(
                    duracionMinutos));

        if (horaInicio < horario.HoraApertura ||
            horaFin > horario.HoraCierre)
        {
            throw new InvalidOperationException(
                "El turno seleccionado queda fuera del horario de atención.");
        }

        var inicioDia =
            fechaHora.Date;

        var finDia =
            inicioDia.AddDays(1);

        var turnosDelDia =
            await context.Turnos
                .AsNoTracking()
                .Include(x => x.Servicio)
                .Where(x =>
                    x.FechaHora >= inicioDia &&
                    x.FechaHora < finDia &&
                    x.Estado != EstadoTurno.Cancelado)
                .ToListAsync();

        var nuevoInicio =
            fechaHora;

        var nuevoFin =
            fechaHora.AddMinutes(
                duracionMinutos);

        var existeSuperposicion =
            turnosDelDia.Any(x =>
            {
                var existenteInicio =
                    x.FechaHora;

                var existenteFin =
                    x.FechaHora.AddMinutes(
                        x.Servicio.DuracionMinutos);

                return nuevoInicio < existenteFin &&
                       nuevoFin > existenteInicio;
            });

        if (existeSuperposicion)
        {
            throw new InvalidOperationException(
                "El horario seleccionado ya se encuentra ocupado.");
        }
    }

    private static bool EsTransicionValida(
        EstadoTurno actual,
        EstadoTurno nuevo) =>
        actual switch
        {
            EstadoTurno.Pendiente =>
                nuevo is EstadoTurno.Confirmado
                    or EstadoTurno.Cancelado,

            EstadoTurno.Confirmado =>
                nuevo is EstadoTurno.Completado
                    or EstadoTurno.Cancelado,

            _ => false
        };

    private static void ValidarPaginacion(
        ref int pagina,
        ref int tamanioPagina)
    {
        if (pagina < 1)
            pagina = 1;

        tamanioPagina =
            Math.Clamp(
                tamanioPagina,
                1,
                100);
    }

    private static string? NormalizarOpcional(
        string? valor) =>
        string.IsNullOrWhiteSpace(valor)
            ? null
            : valor.Trim();
}
