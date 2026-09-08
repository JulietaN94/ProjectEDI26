namespace Lavadero.Application.DTOs;

public sealed record HorarioAtencionDto(
    int Id,
    DayOfWeek DiaSemana,
    TimeSpan HoraApertura,
    TimeSpan HoraCierre,
    bool Activo);
