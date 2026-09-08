using Lavadero.Application.Common;
using Lavadero.Application.DTOs;
using Lavadero.Application.Models;

namespace Lavadero.Application.Interfaces;

/// <summary>
/// Operaciones de administración de los servicios ofrecidos por el lavadero.
/// </summary>
public interface IServicioService
{
    Task<IReadOnlyList<ServicioDto>> ObtenerActivosAsync();

    Task<ServicioDto?> ObtenerPorIdAsync(int id);

    Task<PagedResult<ServicioDto>> ObtenerPaginadosAsync(
        string? busqueda,
        bool? activo,
        int pagina,
        int tamanioPagina);

    Task<ServicioDto> CrearAsync(GuardarServicioRequest request);

    Task<ServicioDto> ActualizarAsync(int id, GuardarServicioRequest request);

    Task EliminarAsync(int id);
}
