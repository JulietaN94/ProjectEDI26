using Lavadero.Application.DTOs;
using Lavadero.Application.Interfaces;
using Lavadero.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lavadero.Infrastructure.Services;

public sealed class HorarioAtencionService : IHorarioAtencionService
{
    private readonly IDbContextFactory<LavaderoDbContext> _factory;

    public HorarioAtencionService(IDbContextFactory<LavaderoDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<HorarioAtencionDto>> ObtenerTodosAsync()
    {
        await using var context = await _factory.CreateDbContextAsync();

        return await context.HorariosAtencion
            .AsNoTracking()
            .OrderBy(x => x.DiaSemana == DayOfWeek.Sunday ? 7 : (int)x.DiaSemana)
            .Select(x => new HorarioAtencionDto(
                x.Id,
                x.DiaSemana,
                x.HoraApertura,
                x.HoraCierre,
                x.Activo))
            .ToListAsync();
    }
}
