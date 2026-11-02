using Lavadero.Application.Common;
using Lavadero.Application.DTOs;
using Lavadero.Application.Interfaces;
using Lavadero.Application.Models;
using Lavadero.Domain.Entities;
using Lavadero.Domain.Enums;
using Lavadero.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Threading.RateLimiting;

namespace Lavadero.Infrastructure.Services;

public sealed class TurnoService : ITurnoService
{
    private const int PasoMinutos = 30;
    private const int HorasValidezConfirmacion = 24;
    private static readonly PartitionedRateLimiter<string> LimiteReservas =
        PartitionedRateLimiter.Create<string, string>(telefono =>
            RateLimitPartition.GetFixedWindowLimiter(telefono, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromHours(1),
                QueueLimit = 0
            }));

    private readonly IDbContextFactory<LavaderoDbContext> _factory;
    private readonly ILogger<TurnoService> _logger;
    private readonly IAppClock _clock;

    public TurnoService(
        IDbContextFactory<LavaderoDbContext> factory,
        ILogger<TurnoService> logger,
        IAppClock clock)
    {
        _factory = factory;
        _logger = logger;
        _clock = clock;
    }

    public async Task<TurnoDto> CrearReservaAsync(
        CrearReservaRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidarDatosReserva(request);
        ValidarVentanaReserva(request.FechaHora);

        var telefono =
            NormalizarTelefono(request.Telefono);

        using var permiso = await LimiteReservas.AcquireAsync(telefono);
        if (!permiso.IsAcquired)
            throw new InvalidOperationException("Se alcanzó el límite de intentos de reserva para este celular. Intentá nuevamente más tarde.");

        var nombreCompleto =
            request.NombreCompleto.Trim();

        var marca =
            request.Marca.Trim();

        var modelo =
            request.Modelo.Trim();

        await using var context =
            await _factory.CreateDbContextAsync();

        /*
         * El bloqueo se mantiene hasta confirmar/descartar la transacción.
         *
         * La clave está separada por fecha, por lo que dos reservas de
         * días distintos pueden procesarse al mismo tiempo, pero dos
         * reservas del mismo día se validan y guardan una detrás de otra.
         *
         * Esto evita que dos clientes vean el mismo horario como libre y
         * logren guardarlo simultáneamente.
         */
        await using var transaction =
            await context.Database.BeginTransactionAsync();

        await AdquirirBloqueoReservaAsync(
            context,
            transaction,
            request.FechaHora.Date);

        var servicio =
            await context.Servicios
                .FirstOrDefaultAsync(
                    x => x.Id == request.ServicioId &&
                         x.Activo)
            ?? throw new InvalidOperationException(
                "El servicio seleccionado no está disponible.");

        // Se vuelve a validar DENTRO del bloqueo/transacción.
        await ValidarHorarioYDisponibilidadAsync(
            context,
            request.FechaHora,
            servicio.DuracionMinutos);

        /*
         * Para no duplicar clientes por el formato del teléfono,
         * comparamos también contra registros históricos que puedan
         * tener espacios, +, guiones o paréntesis.
         *
         * Ejemplo:
         * 11 1234-5678
         * (11) 1234-5678
         * 1112345678
         *
         * se consideran el mismo número.
         */
        var cliente =
            await context.Clientes
                .Include(x => x.Autos)
                .FirstOrDefaultAsync(x =>
                    x.Telefono
                        .Replace("+", "")
                        .Replace("-", "")
                        .Replace(" ", "")
                        .Replace("(", "")
                        .Replace(")", "") ==
                    telefono);

        if (cliente is null)
        {
            cliente = new Cliente
            {
                NombreCompleto = nombreCompleto,

                // Los clientes nuevos se guardan solamente con dígitos.
                Telefono = telefono,

                FechaAlta = _clock.UtcNow
            };

            context.Clientes.Add(cliente);
        }
        else if (!string.Equals(
                     cliente.NombreCompleto,
                     nombreCompleto,
                     StringComparison.Ordinal))
        {
            cliente.NombreCompleto =
                nombreCompleto;
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
            FechaCreacion = _clock.UtcNow,
            TokenConfirmacion = GenerarTokenSeguro(),
            FechaExpiracionConfirmacion =
                _clock.UtcNow.AddHours(
                    HorasValidezConfirmacion),
            FechaConfirmacion = null,
            TokenCancelacion = GenerarTokenSeguro()
        };

        context.Turnos.Add(turno);

        await context.SaveChangesAsync();
        await transaction.CommitAsync();

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
        var hoy = _clock.LocalToday;

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
                    x.Estado != EstadoTurno.Cancelado &&
                    !(x.Estado == EstadoTurno.Pendiente &&
                      x.FechaExpiracionConfirmacion.HasValue &&
                      x.FechaExpiracionConfirmacion.Value <= _clock.UtcNow))
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

            if (inicio <= _clock.LocalNow)
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
                x.PrecioServicio,
                x.Servicio.DuracionMinutos))
            .FirstOrDefaultAsync();
    }


    public async Task<TurnoDto?> ObtenerPorTokenAsync(
        string token)
    {
        var tokenNormalizado =
            NormalizarToken(token);

        if (tokenNormalizado is null)
        {
            return null;
        }

        await using var context =
            await _factory.CreateDbContextAsync();

        return await QueryTurnos(context)
            .Where(x =>
                x.TokenConfirmacion == tokenNormalizado)
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
            .FirstOrDefaultAsync();
    }


    public async Task<string?> ObtenerTokenConfirmacionAsync(
        int turnoId)
    {
        await using var context =
            await _factory.CreateDbContextAsync();

        return await context.Turnos
            .AsNoTracking()
            .Where(x => x.Id == turnoId)
            .Select(x => x.TokenConfirmacion)
            .FirstOrDefaultAsync();
    }


    public async Task<TurnoDto?> ObtenerPorTokenCancelacionAsync(
        string token)
    {
        var tokenNormalizado =
            NormalizarToken(token);

        if (tokenNormalizado is null)
        {
            return null;
        }

        await using var context =
            await _factory.CreateDbContextAsync();

        return await QueryTurnos(context)
            .Where(x =>
                x.TokenCancelacion == tokenNormalizado)
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
            .FirstOrDefaultAsync();
    }

    public async Task<string?> ObtenerTokenCancelacionAsync(
        int turnoId)
    {
        await using var context =
            await _factory.CreateDbContextAsync();

        return await context.Turnos
            .AsNoTracking()
            .Where(x => x.Id == turnoId)
            .Select(x => x.TokenCancelacion)
            .FirstOrDefaultAsync();
    }

    public async Task CancelarPorTokenAsync(
        string token)
    {
        var tokenNormalizado =
            NormalizarToken(token)
            ?? throw new KeyNotFoundException(
                "El enlace de cancelación no es válido.");

        var ahoraLocal = _clock.LocalNow;

        await using var context =
            await _factory.CreateDbContextAsync();

        /*
         * Igual que la confirmación, la cancelación se ejecuta de forma
         * atómica directamente en SQL.
         *
         * Solo pueden cancelarse turnos Pendientes o Confirmados y
         * únicamente antes de su horario.
         */
        var filasActualizadas =
            await context.Turnos
                .Where(x =>
                    x.TokenCancelacion == tokenNormalizado &&
                    (x.Estado == EstadoTurno.Pendiente ||
                     x.Estado == EstadoTurno.Confirmado) &&
                    x.FechaHora > ahoraLocal)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(
                        x => x.Estado,
                        EstadoTurno.Cancelado));

        if (filasActualizadas == 1)
        {
            _logger.LogInformation(
                "Turno cancelado mediante enlace público.");

            return;
        }

        var turno =
            await context.Turnos
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.TokenCancelacion ==
                    tokenNormalizado)
            ?? throw new KeyNotFoundException(
                "El enlace de cancelación no es válido.");

        // Idempotencia: volver a abrir/cancelar el mismo turno no genera error.
        if (turno.Estado == EstadoTurno.Cancelado)
        {
            return;
        }

        if (turno.Estado == EstadoTurno.Completado)
        {
            throw new InvalidOperationException(
                "El turno ya fue completado y no puede cancelarse.");
        }

        if (turno.FechaHora <= ahoraLocal)
        {
            throw new InvalidOperationException(
                "El turno ya comenzó o pasó y no puede cancelarse.");
        }

        throw new InvalidOperationException(
            "El turno no puede cancelarse en su estado actual.");
    }

    public async Task ConfirmarPorTokenAsync(
        string token)
    {
        var tokenNormalizado =
            NormalizarToken(token)
            ?? throw new KeyNotFoundException(
                "El enlace de confirmación no es válido.");

        var ahoraUtc = _clock.UtcNow;
        var ahoraLocal = _clock.LocalNow;

        await using var context =
            await _factory.CreateDbContextAsync();

        /*
         * La confirmación se realiza en una única operación SQL.
         *
         * Así evitamos que dos solicitudes simultáneas lean el mismo
         * turno Pendiente y lo modifiquen por separado.
         */
        var filasActualizadas =
            await context.Turnos
                .Where(x =>
                    x.TokenConfirmacion == tokenNormalizado &&
                    x.Estado == EstadoTurno.Pendiente &&
                    x.FechaExpiracionConfirmacion.HasValue &&
                    x.FechaExpiracionConfirmacion.Value > ahoraUtc &&
                    x.FechaHora > ahoraLocal)
                .ExecuteUpdateAsync(setters =>
                    setters
                        .SetProperty(
                            x => x.Estado,
                            EstadoTurno.Confirmado)
                        .SetProperty(
                            x => x.FechaConfirmacion,
                            ahoraUtc));

        if (filasActualizadas == 1)
        {
            _logger.LogInformation(
                "Turno confirmado mediante enlace público.");

            return;
        }

        /*
         * Si no se actualizó ninguna fila, consultamos el estado actual
         * únicamente para devolver un mensaje correcto al usuario.
         */
        var turno =
            await context.Turnos
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.TokenConfirmacion ==
                    tokenNormalizado)
            ?? throw new KeyNotFoundException(
                "El enlace de confirmación no es válido.");

        // Idempotencia: volver a abrir un enlace ya confirmado no genera error.
        if (turno.Estado == EstadoTurno.Confirmado)
        {
            return;
        }

        if (turno.Estado == EstadoTurno.Cancelado)
        {
            throw new InvalidOperationException(
                "El turno fue cancelado y ya no puede confirmarse.");
        }

        if (turno.Estado == EstadoTurno.Completado)
        {
            throw new InvalidOperationException(
                "El turno ya fue completado.");
        }

        if (!turno.FechaExpiracionConfirmacion.HasValue ||
            turno.FechaExpiracionConfirmacion.Value <= ahoraUtc)
        {
            throw new InvalidOperationException(
                "El enlace de confirmación venció.");
        }

        if (turno.FechaHora <= ahoraLocal)
        {
            throw new InvalidOperationException(
                "El turno ya pasó y no puede confirmarse.");
        }

        throw new InvalidOperationException(
            "El turno no se encuentra pendiente de confirmación.");
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

        var ahora = _clock.LocalNow;

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

        if (turno.Estado == EstadoTurno.Pendiente &&
            nuevoEstado == EstadoTurno.Confirmado &&
            turno.FechaExpiracionConfirmacion.HasValue &&
            turno.FechaExpiracionConfirmacion.Value <= _clock.UtcNow)
            throw new InvalidOperationException("La reserva venció. Es necesario crear una nueva reserva en un horario disponible.");

        if (!EstadoTurnoTransiciones.EsValida(
                turno.Estado,
                nuevoEstado))
        {
            throw new InvalidOperationException(
                $"No se puede cambiar el turno de {turno.Estado} a {nuevoEstado}.");
        }

        var ahoraUtc = _clock.UtcNow;
        var actualizados = await context.Turnos
            .Where(x => x.Id == turnoId && x.Estado == estadoAnterior &&
                (nuevoEstado != EstadoTurno.Confirmado ||
                 !x.FechaExpiracionConfirmacion.HasValue ||
                 x.FechaExpiracionConfirmacion.Value > ahoraUtc))
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.Estado, nuevoEstado));
        if (actualizados != 1)
            throw new InvalidOperationException("La reserva venció o cambió de estado. Actualizá la lista e intentá nuevamente.");

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

    private void ValidarVentanaReserva(
        DateTime fechaHora)
    {
        var ahora = _clock.LocalNow;
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
                    x.PrecioServicio,
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

    private async Task
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
                    x.Estado != EstadoTurno.Cancelado &&
                    !(x.Estado == EstadoTurno.Pendiente &&
                      x.FechaExpiracionConfirmacion.HasValue &&
                      x.FechaExpiracionConfirmacion.Value <= _clock.UtcNow))
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

    private static void ValidarDatosReserva(
        CrearReservaRequest request)
    {
        var resultados =
            new List<ValidationResult>();

        var contexto =
            new ValidationContext(request);

        if (!Validator.TryValidateObject(
                request,
                contexto,
                resultados,
                validateAllProperties: true))
        {
            throw new InvalidOperationException(
                resultados.FirstOrDefault()?.ErrorMessage ??
                "Los datos ingresados para la reserva no son válidos.");
        }
    }

    private static string NormalizarTelefono(
        string? telefono)
    {
        if (string.IsNullOrWhiteSpace(telefono))
        {
            throw new InvalidOperationException(
                "El teléfono es obligatorio.");
        }

        var normalizado =
            new string(
                telefono
                    .Where(char.IsDigit)
                    .ToArray());

        /*
         * 15 dígitos es el máximo internacional E.164.
         * Dejamos un mínimo amplio para no atar el sistema a un único
         * formato local.
         */
        if (normalizado.Length is < 6 or > 15)
        {
            throw new InvalidOperationException(
                "El teléfono debe contener entre 6 y 15 dígitos.");
        }

        return normalizado;
    }

    private static async Task AdquirirBloqueoReservaAsync(
        LavaderoDbContext context,
        IDbContextTransaction transaction,
        DateTime fecha)
    {
        var connection =
            context.Database.GetDbConnection();

        await using var command =
            connection.CreateCommand();

        command.Transaction =
            transaction.GetDbTransaction();

        command.CommandText = """
            DECLARE @resultado int;

            EXEC @resultado = sys.sp_getapplock
                @Resource = @recurso,
                @LockMode = 'Exclusive',
                @LockOwner = 'Transaction',
                @LockTimeout = 10000;

            SELECT @resultado;
            """;

        var parametro =
            command.CreateParameter();

        parametro.ParameterName =
            "@recurso";

        parametro.Value =
            $"Lavadero:Reserva:{fecha:yyyyMMdd}";

        command.Parameters.Add(
            parametro);

        var resultado =
            Convert.ToInt32(
                await command.ExecuteScalarAsync());

        // sp_getapplock: cualquier código negativo indica fallo.
        if (resultado < 0)
        {
            throw new InvalidOperationException(
                "No se pudo asegurar el horario seleccionado. " +
                "Intentá nuevamente.");
        }
    }

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

    /// <summary>
    /// Genera un token criptográficamente seguro de 64 caracteres hexadecimales.
    /// No se utiliza el Id del turno para evitar enlaces predecibles.
    /// </summary>
    private static string GenerarTokenSeguro()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);

        return Convert
            .ToHexString(bytes)
            .ToLowerInvariant();
    }

    private static string? NormalizarToken(
        string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var normalizado =
            token.Trim().ToLowerInvariant();

        // Los tokens de Fase 8.2 son 32 bytes expresados
        // como 64 caracteres hexadecimales.
        if (normalizado.Length != 64 ||
            normalizado.Any(c =>
                !Uri.IsHexDigit(c)))
        {
            return null;
        }

        return normalizado;
    }

    private static string? NormalizarOpcional(
        string? valor) =>
        string.IsNullOrWhiteSpace(valor)
            ? null
            : valor.Trim();
}
