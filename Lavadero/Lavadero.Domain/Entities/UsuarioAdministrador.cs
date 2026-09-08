using System.ComponentModel.DataAnnotations;

namespace Lavadero.Domain.Entities;

public class UsuarioAdministrador
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Usuario { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}
