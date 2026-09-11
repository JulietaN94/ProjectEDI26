using System.ComponentModel.DataAnnotations;
using Lavadero.Domain.Enums;

namespace Lavadero.Domain.Entities;

public class Turno
{
    public int Id { get; set; }

    public int AutoId { get; set; }

    public int ServicioId { get; set; }

    public DateTime FechaHora { get; set; }

    public EstadoTurno Estado { get; set; } = EstadoTurno.Pendiente;

    /// <summary>
    /// Precio del servicio al momento de crear la reserva.
    /// Permite conservar un historial correcto aunque luego cambie el precio del servicio.
    /// </summary>
    public decimal PrecioServicio { get; set; }

    [MaxLength(500)]
    public string? Observaciones { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Token público e impredecible utilizado para confirmar el turno
    /// sin necesidad de que el cliente tenga una cuenta.
    /// </summary>
    [MaxLength(64)]
    public string? TokenConfirmacion { get; set; }

    /// <summary>
    /// Fecha y hora hasta la que el enlace de confirmación es válido.
    /// Se almacenará en UTC.
    /// </summary>
    public DateTime? FechaExpiracionConfirmacion { get; set; }

    /// <summary>
    /// Fecha y hora en la que el cliente confirmó el turno mediante el enlace.
    /// Se almacenará en UTC.
    /// </summary>
    public DateTime? FechaConfirmacion { get; set; }

    /// <summary>
    /// Token público e impredecible utilizado para cancelar el turno
    /// sin necesidad de que el cliente tenga una cuenta.
    /// </summary>
    [MaxLength(64)]
    public string? TokenCancelacion { get; set; }

    public Auto Auto { get; set; } = null!;

    public Servicio Servicio { get; set; } = null!;
}
