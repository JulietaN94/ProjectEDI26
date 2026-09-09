using Lavadero.Application.Common;
using Lavadero.Application.DTOs;

namespace Lavadero.Application.Interfaces;

/// <summary>
/// Define las consultas disponibles para trabajar con clientes del lavadero.
/// </summary>
public interface IClienteService
{
    /// <summary>
    /// Obtiene un cliente por su identificador.
    /// </summary>
    /// <param name="id">Identificador del cliente.</param>
    /// <returns>El cliente encontrado o <see langword="null"/>.</returns>
    Task<ClienteDto?> ObtenerPorIdAsync(int id);

    /// <summary>
    /// Busca un cliente utilizando su número de teléfono.
    /// </summary>
    /// <param name="telefono">Teléfono del cliente.</param>
    /// <returns>El cliente encontrado o <see langword="null"/>.</returns>
    Task<ClienteDto?> ObtenerPorTelefonoAsync(string telefono);

    /// <summary>
    /// Obtiene clientes de forma paginada y permite aplicar una búsqueda.
    /// </summary>
    /// <param name="busqueda">Texto opcional de búsqueda.</param>
    /// <param name="pagina">Número de página comenzando en 1.</param>
    /// <param name="tamanioPagina">Cantidad de elementos por página.</param>
    /// <returns>Resultado paginado de clientes.</returns>
    Task<PagedResult<ClienteDto>> ObtenerPaginadosAsync(
        string? busqueda,
        int pagina,
        int tamanioPagina);
}
