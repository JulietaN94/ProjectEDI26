using Lavadero.Application.Common;
using Lavadero.Application.DTOs;
using Lavadero.Application.Models;
using Lavadero.Domain.Enums;

namespace Lavadero.Application.Interfaces;

/// <summary>
/// Define los casos de uso para reservar y administrar turnos.
/// </summary>
public interface ITurnoService
{
    /// <summary>
    /// Crea una reserva validando fecha, horario y disponibilidad.
    /// </summary>
    Task<TurnoDto> CrearReservaAsync(CrearReservaRequest request);

    /// <summary>
    /// Obtiene los horarios realmente disponibles para un servicio y una fecha.
    /// Los horarios ocupados o que se superponen con otro turno no se devuelven.
    /// </summary>
    Task<IReadOnlyList<TimeSpan>> ObtenerHorariosDisponiblesAsync(
        int servicioId,
        DateTime fecha);

    Task<TurnoDto?> ObtenerPorIdAsync(int id);

    /// <summary>
    /// Obtiene un turno a partir de su token público de confirmación.
    /// Devuelve null cuando el token no existe.
    /// </summary>
    Task<TurnoDto?> ObtenerPorTokenAsync(string token);

    /// <summary>
    /// Obtiene el token de confirmación asociado a un turno.
    /// Se utiliza para construir el enlace público después de crear la reserva.
    /// </summary>
    Task<string?> ObtenerTokenConfirmacionAsync(int turnoId);

    /// <summary>
    /// Obtiene un turno a partir de su token público de cancelación.
    /// Devuelve null cuando el token no existe.
    /// </summary>
    Task<TurnoDto?> ObtenerPorTokenCancelacionAsync(string token);

    /// <summary>
    /// Obtiene el token de cancelación asociado a un turno.
    /// </summary>
    Task<string?> ObtenerTokenCancelacionAsync(int turnoId);

    /// <summary>
    /// Cancela un turno pendiente o confirmado mediante su token público.
    /// </summary>
    Task CancelarPorTokenAsync(string token);

    /// <summary>
    /// Confirma un turno pendiente mediante su token público.
    /// Valida existencia, vencimiento, fecha del turno y estado actual.
    /// </summary>
    Task ConfirmarPorTokenAsync(string token);

    Task<PagedResult<TurnoDto>> ObtenerPaginadosAsync(
        string? busqueda,
        EstadoTurno? estado,
        int pagina,
        int tamanioPagina);

    Task<PagedResult<TurnoDto>> ObtenerHistorialAsync(
        string? busqueda,
        DateTime? desde,
        DateTime? hasta,
        EstadoTurno? estado,
        int pagina,
        int tamanioPagina);

    /// <summary>
    /// Obtiene la primera fecha en la que se registró actividad real de turnos.
    /// Se utiliza como límite inferior del filtro histórico.
    /// </summary>
    Task<DateTime?> ObtenerPrimeraFechaRegistroAsync();

    Task CambiarEstadoAsync(
        int turnoId,
        EstadoTurno nuevoEstado);

    Task CancelarAsync(int turnoId);
}
