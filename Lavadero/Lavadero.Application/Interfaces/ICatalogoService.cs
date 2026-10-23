using Lavadero.Application.DTOs;
using Lavadero.Application.Models;
using Lavadero.Domain.Entities;

namespace Lavadero.Application.Interfaces;

public interface ICatalogoService
{
    Task<List<ProductoDto>> ProductosAsync(bool todos = false);
    Task GuardarProductoAsync(GuardarProductoRequest producto);
    Task<int> CrearPedidoAsync(CrearPedidoRequest request);
    Task<List<PedidoDto>> PedidosAsync();
    Task CambiarEstadoAsync(int id, EstadoPedido estado);
}
