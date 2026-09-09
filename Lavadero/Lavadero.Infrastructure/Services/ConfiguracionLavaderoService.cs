using Lavadero.Application.DTOs;
using Lavadero.Application.Interfaces;
using Lavadero.Application.Models;
using Lavadero.Domain.Entities;
using Lavadero.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lavadero.Infrastructure.Services;

public sealed class ConfiguracionLavaderoService
    : IConfiguracionLavaderoService
{
    private const int ConfiguracionId = 1;

    private readonly IDbContextFactory<LavaderoDbContext> _factory;
    private readonly ILogger<ConfiguracionLavaderoService> _logger;

    public ConfiguracionLavaderoService(
        IDbContextFactory<LavaderoDbContext> factory,
        ILogger<ConfiguracionLavaderoService> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public async Task<ConfiguracionLavaderoDto> ObtenerAsync()
    {
        await using var context =
            await _factory.CreateDbContextAsync();

        var configuracion =
            await context.ConfiguracionLavadero
                .AsNoTracking()
                .Where(x => x.Id == ConfiguracionId)
                .Select(x => new ConfiguracionLavaderoDto(
                    x.Id,
                    x.Nombre,
                    x.Telefono,
                    x.Direccion,
                    x.WhatsApp,
                    x.LogoBase64 != null && x.LogoBase64 != "",
                    x.LogoContentType))
                .FirstOrDefaultAsync();

        return configuracion
            ?? new ConfiguracionLavaderoDto(
                ConfiguracionId,
                "Mi Lavadero",
                null,
                null,
                null,
                false,
                null);
    }

    public async Task<LogoLavaderoDto?> ObtenerLogoAsync()
    {
        await using var context =
            await _factory.CreateDbContextAsync();

        var logo =
            await context.ConfiguracionLavadero
                .AsNoTracking()
                .Where(x => x.Id == ConfiguracionId)
                .Select(x => new
                {
                    x.LogoBase64,
                    x.LogoContentType
                })
                .FirstOrDefaultAsync();

        if (logo is null ||
            string.IsNullOrWhiteSpace(logo.LogoBase64) ||
            string.IsNullOrWhiteSpace(logo.LogoContentType))
        {
            return null;
        }

        try
        {
            return new LogoLavaderoDto(
                Convert.FromBase64String(logo.LogoBase64),
                logo.LogoContentType);
        }
        catch (FormatException ex)
        {
            _logger.LogError(
                ex,
                "El logo almacenado no contiene Base64 válido.");

            return null;
        }
    }

    public async Task<ConfiguracionLavaderoDto> ActualizarAsync(
        GuardarConfiguracionLavaderoRequest request)
    {
        await using var context =
            await _factory.CreateDbContextAsync();

        var configuracion =
            await ObtenerOCrearEntidadAsync(context);

        configuracion.Nombre =
            request.Nombre.Trim();

        configuracion.Telefono =
            Normalizar(request.Telefono);

        configuracion.Direccion =
            Normalizar(request.Direccion);

        configuracion.WhatsApp =
            Normalizar(request.WhatsApp);

        await context.SaveChangesAsync();

        return CrearDto(configuracion);
    }

    public async Task GuardarLogoAsync(
        byte[] contenido,
        string contentType)
    {
        ArgumentNullException.ThrowIfNull(contenido);

        if (contenido.Length == 0)
        {
            throw new InvalidOperationException(
                "La imagen seleccionada está vacía.");
        }

        await using var context =
            await _factory.CreateDbContextAsync();

        var configuracion =
            await ObtenerOCrearEntidadAsync(context);

        configuracion.LogoBase64 =
            Convert.ToBase64String(contenido);

        configuracion.LogoContentType =
            contentType;

        await context.SaveChangesAsync();

        _logger.LogInformation(
            "Logo del lavadero actualizado. Tamaño: {Bytes} bytes.",
            contenido.Length);
    }

    public async Task EliminarLogoAsync()
    {
        await using var context =
            await _factory.CreateDbContextAsync();

        var configuracion =
            await context.ConfiguracionLavadero
                .FirstOrDefaultAsync(
                    x => x.Id == ConfiguracionId);

        if (configuracion is null)
            return;

        configuracion.LogoBase64 = null;
        configuracion.LogoContentType = null;

        await context.SaveChangesAsync();
    }

    private static async Task<ConfiguracionLavadero>
        ObtenerOCrearEntidadAsync(
            LavaderoDbContext context)
    {
        var configuracion =
            await context.ConfiguracionLavadero
                .FirstOrDefaultAsync(
                    x => x.Id == ConfiguracionId);

        if (configuracion is not null)
            return configuracion;

        configuracion = new ConfiguracionLavadero
        {
            Id = ConfiguracionId,
            Nombre = "Mi Lavadero"
        };

        context.ConfiguracionLavadero.Add(configuracion);

        return configuracion;
    }

    private static ConfiguracionLavaderoDto CrearDto(
        ConfiguracionLavadero configuracion) =>
        new(
            configuracion.Id,
            configuracion.Nombre,
            configuracion.Telefono,
            configuracion.Direccion,
            configuracion.WhatsApp,
            !string.IsNullOrWhiteSpace(
                configuracion.LogoBase64),
            configuracion.LogoContentType);

    private static string? Normalizar(string? valor) =>
        string.IsNullOrWhiteSpace(valor)
            ? null
            : valor.Trim();
}
