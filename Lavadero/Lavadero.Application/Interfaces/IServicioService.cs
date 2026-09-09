using Lavadero.Application.Common;
using Lavadero.Application.DTOs;
using Lavadero.Application.Models;

namespace Lavadero.Application.Interfaces;

/// <summary>
/// Define las operaciones de consulta y administración de servicios del lavadero.
/// </summary>
public interface IServicioService
{
    /// <summary>
    /// Obtiene todos los servicios activos disponibles para reservas.
    /// </summary>
    /// <returns>Lista de servicios activos.</returns>
    Task<IReadOnlyList<ServicioDto>> ObtenerActivosAsync();

    /// <summary>
    /// Obtiene un servicio por identificador.
    /// </summary>
    /// <param name="id">Identificador del servicio.</param>
    /// <returns>Servicio encontrado o <see langword="null"/>.</returns>
    Task<ServicioDto?> ObtenerPorIdAsync(int id);

    /// <summary>
    /// Obtiene los servicios de forma paginada.
    /// </summary>
    /// <param name="busqueda">Texto opcional para buscar por nombre o descripción.</param>
    /// <param name="activo">Filtro opcional de estado.</param>
    /// <param name="pagina">Número de página comenzando en 1.</param>
    /// <param name="tamanioPagina">Cantidad de registros por página.</param>
    /// <returns>Resultado paginado de servicios.</returns>
    Task<PagedResult<ServicioDto>> ObtenerPaginadosAsync(
        string? busqueda,
        bool? activo,
        int pagina,
        int tamanioPagina);

    /// <summary>
    /// Crea un nuevo servicio.
    /// </summary>
    /// <param name="request">Datos del servicio.</param>
    /// <returns>Servicio creado con su identificador.</returns>
    Task<ServicioDto> CrearAsync(
        GuardarServicioRequest request);

    /// <summary>
    /// Actualiza un servicio existente.
    /// </summary>
    /// <param name="id">Identificador del servicio.</param>
    /// <param name="request">Nuevos datos.</param>
    /// <returns>Servicio actualizado.</returns>
    Task<ServicioDto> ActualizarAsync(
        int id,
        GuardarServicioRequest request);

    /// <summary>
    /// Elimina un servicio cuando no posee turnos asociados.
    /// </summary>
    /// <param name="id">Identificador del servicio.</param>
    /// <exception cref="InvalidOperationException">
    /// Se produce cuando el servicio posee turnos asociados.
    /// </exception>
    Task EliminarAsync(int id);
}
