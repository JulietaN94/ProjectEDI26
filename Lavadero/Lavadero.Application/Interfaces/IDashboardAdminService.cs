using Lavadero.Application.DTOs;

namespace Lavadero.Application.Interfaces;

/// <summary>
/// Proporciona los indicadores principales del panel administrativo.
/// </summary>
public interface IDashboardAdminService
{
    /// <summary>
    /// Obtiene un resumen de actividad para el dashboard.
    /// </summary>
    /// <returns>Indicadores actuales del lavadero.</returns>
    Task<DashboardAdminDto> ObtenerResumenAsync();
}
