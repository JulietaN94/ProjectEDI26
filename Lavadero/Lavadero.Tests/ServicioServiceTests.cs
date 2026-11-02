using Lavadero.Application.Models;
using Lavadero.Domain.Entities;
using Lavadero.Infrastructure.Data;
using Lavadero.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Lavadero.Tests;

public sealed class ServicioServiceTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private DbContextOptions<LavaderoDbContext> options = default!;

    public async Task InitializeAsync()
    {
        await connection.OpenAsync();
        options = new DbContextOptionsBuilder<LavaderoDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new LavaderoDbContext(options);
        await db.Database.EnsureCreatedAsync();
        db.Servicios.AddRange(
            ServicioDemo("Demo Premium A", 1000m),
            ServicioDemo("Demo Premium B", 2000m),
            ServicioDemo("Demo Premium C", 3000m),
            ServicioDemo("Demo Básico", 500m));
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task BuscaYPaginaContraLaBaseDeDatos()
    {
        var servicio = CrearServicio();

        var pagina = await servicio.ObtenerPaginadosAsync(
            "Demo Premium",
            activo: true,
            pagina: 1,
            tamanioPagina: 2);

        Assert.Equal(3, pagina.TotalItems);
        Assert.Equal(2, pagina.Items.Count);
        Assert.Equal(2, pagina.TotalPages);
        Assert.All(pagina.Items, item =>
            Assert.Contains("Premium", item.Nombre));
    }

    [Fact]
    public async Task EjecutaCrudCompleto()
    {
        var servicio = CrearServicio();
        var creado = await servicio.CrearAsync(new GuardarServicioRequest
        {
            Nombre = "Demo CRUD",
            Descripcion = "Servicio creado por la prueba.",
            Precio = 4500m,
            DuracionMinutos = 30
        });

        var actualizado = await servicio.ActualizarAsync(
            creado.Id,
            new GuardarServicioRequest
            {
                Nombre = "Demo CRUD actualizado",
                Precio = 5000m,
                DuracionMinutos = 45
            });

        Assert.Equal(5000m, actualizado.Precio);
        await servicio.EliminarAsync(creado.Id);
        Assert.Null(await servicio.ObtenerPorIdAsync(creado.Id));
    }

    public async Task DisposeAsync() => await connection.DisposeAsync();

    private ServicioService CrearServicio() =>
        new(
            new TestDbContextFactory(options),
            NullLogger<ServicioService>.Instance);

    private static Servicio ServicioDemo(string nombre, decimal precio) => new()
    {
        Nombre = nombre,
        Descripcion = "Dato de prueba",
        Precio = precio,
        DuracionMinutos = 30,
        Activo = true
    };

    private sealed class TestDbContextFactory(
        DbContextOptions<LavaderoDbContext> options)
        : IDbContextFactory<LavaderoDbContext>
    {
        public LavaderoDbContext CreateDbContext() => new(options);
    }
}
