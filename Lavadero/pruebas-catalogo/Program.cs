using Lavadero.Application.Models;
using Lavadero.Domain.Entities;
using Lavadero.Infrastructure.Data;
using Lavadero.Infrastructure.Services;
using Lavadero.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;

var options = new DbContextOptionsBuilder<LavaderoDbContext>().UseSqlServer("Server=localhost;Database=LavaderoCatalogoPrueba;Trusted_Connection=True;TrustServerCertificate=True").Options;
var clock = new ArgentinaAppClock(TimeProvider.System);
var service = new CatalogoService(new Factory(options), clock);
var producto = new GuardarProductoRequest { Codigo = "TEST-" + Guid.NewGuid().ToString("N"), Nombre = "Producto de prueba", PrecioVenta = 120.50m };
await service.GuardarProductoAsync(producto);
var id = await service.CrearPedidoAsync(new CrearPedidoRequest { Nombre = "Prueba", Apellido = "Local", Celular = "1123456789", Cantidades = new() { [producto.Id] = 2 } });
producto.PrecioVenta = 200; producto.Activo = false;
await service.GuardarProductoAsync(producto);
var pedido = (await service.PedidosAsync()).Single(p => p.Id == id);
if (pedido.Detalles.Single().PrecioUnitario != 120.50m || pedido.Detalles.Single().Cantidad != 2) throw new Exception("Snapshot incorrecto");
try { await service.CrearPedidoAsync(new CrearPedidoRequest { Nombre = "Prueba", Apellido = "Local", Celular = "1123456789", Cantidades = new() { [producto.Id] = 1 } }); throw new Exception("Se aceptó producto inactivo"); } catch (InvalidOperationException) { }
await service.CambiarEstadoAsync(id, EstadoPedido.EnPreparacion); await service.CambiarEstadoAsync(id, EstadoPedido.Entregado);
try { await service.CambiarEstadoAsync(id, EstadoPedido.Cancelado); throw new Exception("Transición inválida aceptada"); } catch (InvalidOperationException) { }
Console.WriteLine("OK: producto, pedido, precio histórico, desactivación y estados.");

producto.Activo = true;
await service.GuardarProductoAsync(producto);
var concurrente = await service.CrearPedidoAsync(new CrearPedidoRequest { Nombre = "Prueba", Apellido = "Concurrencia", Celular = "1198765432", Cantidades = new() { [producto.Id] = 1 } });
await service.CambiarEstadoAsync(concurrente, EstadoPedido.EnPreparacion);
async Task<bool> IntentarEstado(EstadoPedido estado)
{
    try { await service.CambiarEstadoAsync(concurrente, estado); return true; }
    catch (InvalidOperationException) { return false; }
}
var resultados = await Task.WhenAll(IntentarEstado(EstadoPedido.Entregado), IntentarEstado(EstadoPedido.Cancelado));
if (resultados.Count(x => x) != 1) throw new Exception("Ambos cambios terminales fueron aceptados.");
Console.WriteLine("OK: solo un cambio de estado terminal aceptado ante solicitudes concurrentes.");

var codigoCompartido = "TEST-" + Guid.NewGuid().ToString("N");
async Task<bool> CrearDuplicado()
{
    try
    {
        await service.GuardarProductoAsync(new GuardarProductoRequest { Codigo = codigoCompartido, Nombre = "Prueba simultánea", PrecioVenta = 10 });
        return true;
    }
    catch (InvalidOperationException ex) when (ex.Message.StartsWith("El código ya existe")) { return false; }
}
var duplicados = await Task.WhenAll(CrearDuplicado(), CrearDuplicado());
if (duplicados.Count(x => x) != 1) throw new Exception("El código duplicado no fue rechazado correctamente.");

var servicios = new ServicioService(new Factory(options), Microsoft.Extensions.Logging.Abstractions.NullLogger<ServicioService>.Instance);
try
{
    await servicios.CrearAsync(new GuardarServicioRequest { Nombre = "Inválido", Precio = -1, DuracionMinutos = 0 });
    throw new Exception("El backend aceptó un servicio inválido.");
}
catch (InvalidOperationException) { }
Console.WriteLine("OK: código duplicado concurrente rechazado y validación de servicios en backend.");

var datosServicio = new GuardarServicioRequest
{
    Nombre = "TEST-" + Guid.NewGuid().ToString("N"),
    Precio = 100, DuracionMinutos = 30, Activo = true
};
var servicioPrueba = await servicios.CrearAsync(datosServicio);
datosServicio.DuracionMinutos = 60;
await servicios.ActualizarAsync(servicioPrueba.Id, datosServicio);
await using (var db = new LavaderoDbContext(options))
{
    db.Turnos.Add(new Turno
    {
        ServicioId = servicioPrueba.Id, FechaHora = DateTime.Today.AddDays(1).AddHours(10), PrecioServicio = 100,
        Auto = new Auto { Marca = "Prueba", Modelo = "Local", Cliente = new Cliente { NombreCompleto = "Prueba duración", Telefono = DateTime.UtcNow.Ticks.ToString() } }
    });
    await db.SaveChangesAsync();
}
datosServicio.DuracionMinutos = 90;
try
{
    await servicios.ActualizarAsync(servicioPrueba.Id, datosServicio);
    throw new Exception("Se modificó la duración de un servicio con turnos.");
}
catch (InvalidOperationException ex) when (ex.Message.Contains("duración")) { }
var conservado = await servicios.ObtenerPorIdAsync(servicioPrueba.Id);
if (conservado?.DuracionMinutos != 60) throw new Exception("La duración original no se conservó.");
datosServicio.DuracionMinutos = 60;
datosServicio.Precio = 150;
await servicios.ActualizarAsync(servicioPrueba.Id, datosServicio);
if ((await servicios.ObtenerPorIdAsync(servicioPrueba.Id))?.Precio != 150)
    throw new Exception("Se bloqueó la edición de otros campos.");
Console.WriteLine("OK: duración editable sin turnos, protegida con turnos y otros campos editables.");

var turnos = new TurnoService(new Factory(options), Microsoft.Extensions.Logging.Abstractions.NullLogger<TurnoService>.Instance, clock);
datosServicio.Nombre = "GRATIS-" + Guid.NewGuid().ToString("N");
datosServicio.Precio = 0;
datosServicio.DuracionMinutos = 30;
var gratuito = await servicios.CrearAsync(datosServicio);
DateTime? fechaLibre = null;
for (var dia = DateTime.Today; dia.Month == DateTime.Today.Month && fechaLibre is null; dia = dia.AddDays(1))
{
    var horarios = await turnos.ObtenerHorariosDisponiblesAsync(gratuito.Id, dia);
    if (horarios.Count > 0) fechaLibre = dia.Add(horarios[0]);
}
if (fechaLibre is null) throw new Exception("No hay un horario libre para probar la reserva.");
var reserva = await turnos.CrearReservaAsync(new CrearReservaRequest
{
    NombreCompleto = "Prueba gratuita", Telefono = "1188888888", Marca = "Prueba", Modelo = "Local",
    ServicioId = gratuito.Id, FechaHora = fechaLibre.Value
});
var confirmacion = await turnos.ObtenerTokenConfirmacionAsync(reserva.Id) ?? throw new Exception("Falta token");
var cancelacion = await turnos.ObtenerTokenCancelacionAsync(reserva.Id) ?? throw new Exception("Falta token");
await turnos.ConfirmarPorTokenAsync(confirmacion);
datosServicio.Precio = 500;
await servicios.ActualizarAsync(gratuito.Id, datosServicio);
var vistas = new[] { await turnos.ObtenerPorIdAsync(reserva.Id), await turnos.ObtenerPorTokenAsync(confirmacion), await turnos.ObtenerPorTokenCancelacionAsync(cancelacion) };
if (vistas.Any(x => x is null || x.ServicioPrecio != 0)) throw new Exception("Se perdió el precio gratuito histórico.");
var lista = await turnos.ObtenerPaginadosAsync("Prueba gratuita", null, 1, 100);
if (lista.Items.Single(x => x.Id == reserva.Id).ServicioPrecio != 0) throw new Exception("Precio incorrecto en listado.");
await turnos.CancelarPorTokenAsync(cancelacion);
var historial = await turnos.ObtenerHistorialAsync("Prueba gratuita", null, null, null, 1, 100);
if (historial.Items.Single(x => x.Id == reserva.Id).ServicioPrecio != 0) throw new Exception("Precio incorrecto en historial.");
Console.WriteLine("OK: reserva, confirmación, cancelación y precio gratuito histórico en consultas y listados.");

sealed class Factory(DbContextOptions<LavaderoDbContext> options) : IDbContextFactory<LavaderoDbContext>
{
    public LavaderoDbContext CreateDbContext() => new(options);
}
