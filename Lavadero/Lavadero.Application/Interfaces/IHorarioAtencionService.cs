using Lavadero.Application.DTOs;
using Lavadero.Application.Models;

namespace Lavadero.Application.Interfaces;

/// <summary>
/// Permite consultar y modificar los horarios de atención.
/// </summary>
public interface IHorarioAtencionService
{
    /// <summary>
    /// Obtiene los horarios configurados para todos los días.
    /// </summary>
    /// <returns>Lista de horarios.</returns>
    Task<IReadOnlyList<HorarioAtencionDto>> ObtenerTodosAsync();

    /// <summary>
    /// Actualiza un horario de atención.
    /// </summary>
    /// <param name="id">Identificador del horario.</param>
    /// <param name="request">Datos del horario.</param>
    Task ActualizarAsync(
        int id,
        GuardarHorarioAtencionRequest request);
}
