using Lavadero.Application.Common;
using Lavadero.Application.DTOs;
using Lavadero.Application.Models;
using Lavadero.Domain.Enums;

namespace Lavadero.Application.Interfaces;

/// <summary>
/// Caso de uso principal de reserva y administración de turnos.
/// </summary>
public interface ITurnoService
{
    Task<TurnoDto> CrearReservaAsync(CrearReservaRequest request);

    Task<TurnoDto?> ObtenerPorIdAsync(int id);

    Task<PagedResult<TurnoDto>> ObtenerPaginadosAsync(
        string? busqueda,
        EstadoTurno? estado,
        int pagina,
        int tamanioPagina);

    Task CambiarEstadoAsync(int turnoId, EstadoTurno nuevoEstado);

    Task CancelarAsync(int turnoId);
}
