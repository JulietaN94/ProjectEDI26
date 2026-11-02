using Lavadero.Application.DTOs;
using Lavadero.Application.Models;
using Lavadero.Domain.Entities;

namespace Lavadero.Application.Interfaces;

/// <summary>
/// Define los casos de uso del catálogo de productos y de los pedidos públicos.
/// </summary>
public interface ICatalogoService
{
    /// <summary>Obtiene los productos disponibles para el catálogo o la administración.</summary>
    /// <param name="todos">Incluye productos inactivos cuando es <see langword="true"/>.</param>
    /// <param name="incluirImagenes">Incluye el contenido binario de las imágenes.</param>
    Task<List<ProductoDto>> ProductosAsync(bool todos = false, bool incluirImagenes = false);

    /// <summary>Obtiene los productos con mayor cantidad pedida para destacarlos en el inicio.</summary>
    /// <param name="cantidad">Cantidad máxima de productos.</param>
    Task<List<ProductoDto>> ProductosDestacadosAsync(int cantidad = 3);

    /// <summary>Obtiene la imagen de un producto activo.</summary>
    /// <param name="id">Identificador del producto.</param>
    Task<ProductoImagenDto?> ObtenerImagenProductoAsync(int id);

    /// <summary>Crea o actualiza un producto del catálogo.</summary>
    /// <param name="producto">Datos validados del producto.</param>
    Task GuardarProductoAsync(GuardarProductoRequest producto);

    /// <summary>Crea un pedido público y devuelve su identificador.</summary>
    /// <param name="request">Cliente y cantidades seleccionadas.</param>
    Task<int> CrearPedidoAsync(CrearPedidoRequest request);

    /// <summary>Obtiene los pedidos más recientes para la administración.</summary>
    Task<List<PedidoDto>> PedidosAsync();

    /// <summary>Actualiza el estado de un pedido respetando sus transiciones permitidas.</summary>
    /// <param name="id">Identificador del pedido.</param>
    /// <param name="estado">Nuevo estado.</param>
    Task CambiarEstadoAsync(int id, EstadoPedido estado);
}
