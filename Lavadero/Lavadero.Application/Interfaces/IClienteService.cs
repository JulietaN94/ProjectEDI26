using Lavadero.Application.Common;
using Lavadero.Application.DTOs;

namespace Lavadero.Application.Interfaces;

/// <summary>
/// Consultas de clientes utilizadas por la administración del lavadero.
/// </summary>
public interface IClienteService
{
    Task<ClienteDto?> ObtenerPorIdAsync(int id);

    Task<ClienteDto?> ObtenerPorTelefonoAsync(string telefono);

    Task<PagedResult<ClienteDto>> ObtenerPaginadosAsync(
        string? busqueda,
        int pagina,
        int tamanioPagina);
}
