using Lavadero.Application.DTOs;

namespace Lavadero.Web.State;

/// <summary>
/// Estado de UI para compartir la última reserva realizada dentro del circuito Blazor.
/// </summary>
public sealed class ReservaState
{
    public TurnoDto? UltimaReserva { get; private set; }

    public event Action? OnChange;

    public void EstablecerUltimaReserva(TurnoDto turno)
    {
        UltimaReserva = turno;
        NotificarCambio();
    }

    public void Limpiar()
    {
        UltimaReserva = null;
        NotificarCambio();
    }

    private void NotificarCambio() => OnChange?.Invoke();
}
