using System.ComponentModel.DataAnnotations;

namespace Lavadero.Domain.Entities;

public class Servicio
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Descripcion { get; set; }

    [Range(typeof(decimal), "0", "999999999.99")]
    public decimal Precio { get; set; }

    [Range(1, 480)]
    public int DuracionMinutos { get; set; }

    public bool Activo { get; set; } = true;

    public ICollection<Turno> Turnos { get; set; } = new List<Turno>();
}
