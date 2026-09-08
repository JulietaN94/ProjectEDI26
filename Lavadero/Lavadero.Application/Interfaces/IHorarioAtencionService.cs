using Lavadero.Application.DTOs;

namespace Lavadero.Application.Interfaces;

/// <summary>
/// Permite consultar los horarios configurados para cada día de la semana.
/// </summary>
public interface IHorarioAtencionService
{
    Task<IReadOnlyList<HorarioAtencionDto>> ObtenerTodosAsync();
}
