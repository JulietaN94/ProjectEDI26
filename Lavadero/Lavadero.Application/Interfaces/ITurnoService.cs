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
