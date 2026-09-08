using System.ComponentModel.DataAnnotations;

namespace Lavadero.Domain.Entities;

public class Auto
{
    public int Id { get; set; }

    public int ClienteId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Marca { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Modelo { get; set; } = string.Empty;

    public Cliente Cliente { get; set; } = null!;

    public ICollection<Turno> Turnos { get; set; } = new List<Turno>();
}
