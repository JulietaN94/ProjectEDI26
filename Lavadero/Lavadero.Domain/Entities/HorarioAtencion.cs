namespace Lavadero.Domain.Entities;

public class HorarioAtencion
{
    public int Id { get; set; }

    public DayOfWeek DiaSemana { get; set; }

    public TimeSpan HoraApertura { get; set; }

    public TimeSpan HoraCierre { get; set; }

    public bool Activo { get; set; } = true;
}
