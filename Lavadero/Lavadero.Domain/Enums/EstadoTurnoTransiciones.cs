namespace Lavadero.Domain.Enums;

/// <summary>
/// Centraliza las transiciones de estado permitidas para un turno.
/// </summary>
public static class EstadoTurnoTransiciones
{
    /// <summary>
    /// Indica si un turno puede pasar del estado actual al estado solicitado.
    /// </summary>
    public static bool EsValida(EstadoTurno actual, EstadoTurno nuevo) =>
        actual switch
        {
            EstadoTurno.Pendiente =>
                nuevo is EstadoTurno.Confirmado or EstadoTurno.Cancelado,
            EstadoTurno.Confirmado =>
                nuevo is EstadoTurno.Completado or EstadoTurno.Cancelado,
            _ => false
        };
}
