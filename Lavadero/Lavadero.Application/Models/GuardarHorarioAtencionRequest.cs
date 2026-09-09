namespace Lavadero.Application.Models;

public sealed class GuardarHorarioAtencionRequest
{
    public TimeSpan HoraApertura { get; set; }

    public TimeSpan HoraCierre { get; set; }

    public bool Activo { get; set; }
}
