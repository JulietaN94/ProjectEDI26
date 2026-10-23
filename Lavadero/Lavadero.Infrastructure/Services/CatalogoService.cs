using Lavadero.Domain.Entities;
using Lavadero.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;

namespace Lavadero.Infrastructure.Services;

public sealed class CatalogoService(IDbContextFactory<LavaderoDbContext> factory)
{
    private static readonly PartitionedRateLimiter<string> Limiter = PartitionedRateLimiter.Create<string, string>(key =>
        RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(10), QueueLimit = 0 }));
    public async Task<List<Producto>> ProductosAsync(bool todos = false)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Productos.AsNoTracking().Where(p => todos || p.Activo).OrderBy(p => p.Nombre).ToListAsync();
    }

    public async Task GuardarProductoAsync(Producto producto)
    {
        producto.Codigo = producto.Codigo.Trim();
        producto.Nombre = producto.Nombre.Trim();
        if (string.IsNullOrWhiteSpace(producto.Codigo) || producto.Codigo.Length > 50 || string.IsNullOrWhiteSpace(producto.Nombre) || producto.Nombre.Length > 100 || producto.Descripcion?.Length > 500 || producto.Marca?.Length > 100 || producto.PrecioVenta < 0 || producto.PrecioVenta > 999999999.99m || decimal.Round(producto.PrecioVenta, 2) != producto.PrecioVenta)
            throw new InvalidOperationException("Revisá el código, nombre y precio del producto.");
        if (producto.Imagen?.Length > 500_000 || (producto.Imagen != null && producto.ImagenContentType is not ("image/png" or "image/jpeg" or "image/webp")))
            throw new InvalidOperationException("La imagen debe ser PNG, JPEG o WebP de hasta 500 KB.");
        if (producto.Imagen is { } imagen && !ImagenValida(imagen, producto.ImagenContentType))
            throw new InvalidOperationException("El contenido de la imagen no coincide con el formato indicado.");
        await using var db = await factory.CreateDbContextAsync();
        if (await db.Productos.AnyAsync(p => p.Codigo == producto.Codigo && p.Id != producto.Id)) throw new InvalidOperationException("El código ya existe.");
        if (producto.Id == 0) db.Productos.Add(producto);
        else
        {
            var actual = await db.Productos.FindAsync(producto.Id) ?? throw new InvalidOperationException("Producto no encontrado.");
            actual.Codigo = producto.Codigo; actual.Nombre = producto.Nombre; actual.Descripcion = producto.Descripcion; actual.Marca = producto.Marca;
            actual.PrecioVenta = producto.PrecioVenta; actual.Activo = producto.Activo; actual.Imagen = producto.Imagen; actual.ImagenContentType = producto.ImagenContentType;
        }
        await db.SaveChangesAsync();
    }

    private static bool ImagenValida(byte[] imagen, string? tipo) => tipo switch
    {
        "image/png" => imagen.Length >= 8 && imagen.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
        "image/jpeg" => imagen.Length >= 3 && imagen[0] == 0xff && imagen[1] == 0xd8 && imagen[2] == 0xff,
        "image/webp" => imagen.Length >= 12 && imagen.AsSpan(0, 4).SequenceEqual("RIFF"u8) && imagen.AsSpan(8, 4).SequenceEqual("WEBP"u8),
        _ => false
    };

    public async Task<int> CrearPedidoAsync(string nombre, string apellido, string celular, Dictionary<int, int> cantidades)
    {
        nombre = nombre.Trim(); apellido = apellido.Trim(); celular = celular.Trim();
        if (nombre.Length is < 1 or > 100 || apellido.Length is < 1 or > 100 || celular.Length is < 6 or > 30 || celular.Count(char.IsDigit) < 6 || cantidades.Count is < 1 or > 50 || cantidades.Values.Any(c => c is < 1 or > 100))
            throw new InvalidOperationException("Revisá tus datos y las cantidades del pedido.");
        using var permiso = await Limiter.AcquireAsync(new string(celular.Where(char.IsDigit).ToArray()));
        if (!permiso.IsAcquired) throw new InvalidOperationException("Se alcanzó el límite de pedidos para este celular. Intentá nuevamente más tarde.");
        await using var db = await factory.CreateDbContextAsync();
        await using var transaction = await db.Database.BeginTransactionAsync();
        var ids = cantidades.Keys.ToArray();
        var productos = await db.Productos.Where(p => ids.Contains(p.Id) && p.Activo).ToListAsync();
        if (productos.Count != ids.Length) throw new InvalidOperationException("Un producto ya no está disponible; actualizá el catálogo.");
        var pedido = new Pedido { Nombre = nombre, Apellido = apellido, Celular = celular };
        foreach (var p in productos) pedido.Detalles.Add(new DetallePedido { ProductoId = p.Id, Codigo = p.Codigo, Nombre = p.Nombre, PrecioUnitario = p.PrecioVenta, Cantidad = cantidades[p.Id] });
        db.Pedidos.Add(pedido); await db.SaveChangesAsync(); await transaction.CommitAsync(); return pedido.Id;
    }

    public async Task<List<Pedido>> PedidosAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Pedidos.AsNoTracking().Include(p => p.Detalles).OrderByDescending(p => p.Id).Take(200).ToListAsync();
    }

    public async Task CambiarEstadoAsync(int id, EstadoPedido estado)
    {
        await using var db = await factory.CreateDbContextAsync();
        var p = await db.Pedidos.FindAsync(id) ?? throw new InvalidOperationException("Pedido no encontrado.");
        if (!Enum.IsDefined(estado) || !((p.Estado == EstadoPedido.Pendiente && estado is EstadoPedido.EnPreparacion or EstadoPedido.Cancelado) || (p.Estado == EstadoPedido.EnPreparacion && estado is EstadoPedido.Entregado or EstadoPedido.Cancelado))) throw new InvalidOperationException("Cambio de estado no permitido.");
        p.Estado = estado; await db.SaveChangesAsync();
    }
}
