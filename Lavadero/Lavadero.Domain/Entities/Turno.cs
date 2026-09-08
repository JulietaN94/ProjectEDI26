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

    [MaxLength(500)]
    public string? Observaciones { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public Auto Auto { get; set; } = null!;

    public Servicio Servicio { get; set; } = null!;
}
