using Lavadero.Application.DTOs;
using Lavadero.Application.Models;
using Lavadero.Domain.Entities;

namespace Lavadero.Application.Interfaces;

public interface ICatalogoService
{
    Task<List<ProductoDto>> ProductosAsync(bool todos = false, bool incluirImagenes = false);
    Task<List<ProductoDto>> ProductosDestacadosAsync(int cantidad = 3);
    Task<ProductoImagenDto?> ObtenerImagenProductoAsync(int id);
    Task GuardarProductoAsync(GuardarProductoRequest producto);
    Task<int> CrearPedidoAsync(CrearPedidoRequest request);
    Task<List<PedidoDto>> PedidosAsync();
    Task CambiarEstadoAsync(int id, EstadoPedido estado);
}
