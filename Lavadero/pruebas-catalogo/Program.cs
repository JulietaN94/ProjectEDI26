using Lavadero.Domain.Entities;
using Lavadero.Infrastructure.Data;
using Lavadero.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var options = new DbContextOptionsBuilder<LavaderoDbContext>().UseSqlServer("Server=localhost;Database=LavaderoCatalogoPrueba;Trusted_Connection=True;TrustServerCertificate=True").Options;
var service = new CatalogoService(new Factory(options));
var producto = new Producto { Codigo = "TEST-" + Guid.NewGuid().ToString("N"), Nombre = "Producto de prueba", PrecioVenta = 120.50m };
await service.GuardarProductoAsync(producto);
var id = await service.CrearPedidoAsync("Prueba", "Local", "1123456789", new() { [producto.Id] = 2 });
producto.PrecioVenta = 200; producto.Activo = false;
await service.GuardarProductoAsync(producto);
var pedido = (await service.PedidosAsync()).Single(p => p.Id == id);
if (pedido.Detalles.Single().PrecioUnitario != 120.50m || pedido.Detalles.Single().Cantidad != 2) throw new Exception("Snapshot incorrecto");
try { await service.CrearPedidoAsync("Prueba", "Local", "1123456789", new() { [producto.Id] = 1 }); throw new Exception("Se aceptó producto inactivo"); } catch (InvalidOperationException) { }
await service.CambiarEstadoAsync(id, EstadoPedido.EnPreparacion); await service.CambiarEstadoAsync(id, EstadoPedido.Entregado);
try { await service.CambiarEstadoAsync(id, EstadoPedido.Cancelado); throw new Exception("Transición inválida aceptada"); } catch (InvalidOperationException) { }
Console.WriteLine("OK: producto, pedido, precio histórico, desactivación y estados.");

sealed class Factory(DbContextOptions<LavaderoDbContext> options) : IDbContextFactory<LavaderoDbContext>
{
    public LavaderoDbContext CreateDbContext() => new(options);
}
