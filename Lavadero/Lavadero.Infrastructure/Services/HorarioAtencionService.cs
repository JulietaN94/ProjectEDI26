using Lavadero.Application.DTOs;
using Lavadero.Application.Interfaces;
using Lavadero.Application.Models;
using Lavadero.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lavadero.Infrastructure.Services;

public sealed class HorarioAtencionService
    : IHorarioAtencionService
{
    private readonly IDbContextFactory<LavaderoDbContext> _factory;

    public HorarioAtencionService(
        IDbContextFactory<LavaderoDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<HorarioAtencionDto>>
        ObtenerTodosAsync()
    {
        await using var context =
            await _factory.CreateDbContextAsync();

        // DiaSemana se guarda como texto en SQL Server.
        // Ordenamos después de traer los datos para evitar que SQL
        // intente convertir "Monday", "Tuesday", etc. a int.
        var horarios =
            await context.HorariosAtencion
                .AsNoTracking()
                .ToListAsync();

        return horarios
            .OrderBy(x =>
                x.DiaSemana == DayOfWeek.Sunday
                    ? 7
                    : (int)x.DiaSemana)
            .Select(x => new HorarioAtencionDto(
                x.Id,
                x.DiaSemana,
                x.HoraApertura,
                x.HoraCierre,
                x.Activo))
            .ToList();
    }

    public async Task ActualizarAsync(
        int id,
        GuardarHorarioAtencionRequest request)
    {
        await using var context =
            await _factory.CreateDbContextAsync();

        var horario =
            await context.HorariosAtencion.FindAsync(id)
            ?? throw new KeyNotFoundException(
                "No se encontró el horario indicado.");

        if (request.Activo)
        {
            if (request.HoraApertura < TimeSpan.Zero ||
                request.HoraApertura >= TimeSpan.FromDays(1) ||
                request.HoraCierre <= TimeSpan.Zero ||
                request.HoraCierre > TimeSpan.FromDays(1))
            {
                throw new InvalidOperationException(
                    "El horario indicado no es válido.");
            }

            if (request.HoraCierre <= request.HoraApertura)
            {
                throw new InvalidOperationException(
                    "La hora de cierre debe ser posterior a la hora de apertura.");
            }
        }

        horario.Activo = request.Activo;

        horario.HoraApertura =
            request.Activo
                ? request.HoraApertura
                : TimeSpan.Zero;

        horario.HoraCierre =
            request.Activo
                ? request.HoraCierre
                : TimeSpan.Zero;

        await context.SaveChangesAsync();
    }
}
