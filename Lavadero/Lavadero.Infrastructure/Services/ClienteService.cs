using Lavadero.Application.Common;
using Lavadero.Application.DTOs;
using Lavadero.Application.Interfaces;
using Lavadero.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lavadero.Infrastructure.Services;

public sealed class ClienteService : IClienteService
{
    private readonly IDbContextFactory<LavaderoDbContext> _factory;

    public ClienteService(IDbContextFactory<LavaderoDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<ClienteDto?> ObtenerPorIdAsync(int id)
    {
        await using var context = await _factory.CreateDbContextAsync();

        return await context.Clientes
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ClienteDto(
                x.Id,
                x.NombreCompleto,
                x.Telefono,
                x.FechaAlta))
            .FirstOrDefaultAsync();
    }

    public async Task<ClienteDto?> ObtenerPorTelefonoAsync(
        string telefono)
    {
        var telefonoNormalizado =
            NormalizarTelefono(telefono);

        if (string.IsNullOrEmpty(
                telefonoNormalizado))
        {
            return null;
        }

        await using var context =
            await _factory.CreateDbContextAsync();

        return await context.Clientes
            .AsNoTracking()
            .Where(x =>
                x.Telefono
                    .Replace("+", "")
                    .Replace("-", "")
                    .Replace(" ", "")
                    .Replace("(", "")
                    .Replace(")", "") ==
                telefonoNormalizado)
            .Select(x => new ClienteDto(
                x.Id,
                x.NombreCompleto,
                x.Telefono,
                x.FechaAlta))
            .FirstOrDefaultAsync();
    }

    public async Task<PagedResult<ClienteDto>> ObtenerPaginadosAsync(
        string? busqueda,
        int pagina,
        int tamanioPagina)
    {
        ValidarPaginacion(ref pagina, ref tamanioPagina);

        await using var context = await _factory.CreateDbContextAsync();

        var query = context.Clientes.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var patron = $"%{busqueda.Trim()}%";

            query = query.Where(x =>
                EF.Functions.Like(x.NombreCompleto, patron) ||
                EF.Functions.Like(x.Telefono, patron));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.NombreCompleto)
            .Skip((pagina - 1) * tamanioPagina)
            .Take(tamanioPagina)
            .Select(x => new ClienteDto(
                x.Id,
                x.NombreCompleto,
                x.Telefono,
                x.FechaAlta))
            .ToListAsync();

        return new PagedResult<ClienteDto>
        {
            Items = items,
            Page = pagina,
            PageSize = tamanioPagina,
            TotalItems = total
        };
    }

    private static string NormalizarTelefono(
        string? telefono) =>
        string.IsNullOrWhiteSpace(telefono)
            ? string.Empty
            : new string(
                telefono
                    .Where(char.IsDigit)
                    .ToArray());

    private static void ValidarPaginacion(ref int pagina, ref int tamanioPagina)
    {
        if (pagina < 1)
            pagina = 1;

        if (tamanioPagina < 1)
            tamanioPagina = 10;

        if (tamanioPagina > 100)
            tamanioPagina = 100;
    }
}
