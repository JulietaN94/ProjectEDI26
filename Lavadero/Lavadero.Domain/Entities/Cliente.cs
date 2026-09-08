using System.ComponentModel.DataAnnotations;

namespace Lavadero.Domain.Entities;

public class Cliente
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Telefono { get; set; } = string.Empty;

    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;

    public ICollection<Auto> Autos { get; set; } = new List<Auto>();
}
