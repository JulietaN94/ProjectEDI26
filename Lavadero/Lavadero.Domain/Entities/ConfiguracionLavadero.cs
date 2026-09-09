using System.ComponentModel.DataAnnotations;

namespace Lavadero.Domain.Entities;

public class ConfiguracionLavadero
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? Telefono { get; set; }

    [MaxLength(200)]
    public string? Direccion { get; set; }

    [MaxLength(30)]
    public string? WhatsApp { get; set; }

    /// <summary>
    /// Imagen del lavadero codificada en Base64.
    /// Se utiliza para mantener el logo junto con la configuración
    /// sin depender del sistema de archivos del servidor.
    /// </summary>
    public string? LogoBase64 { get; set; }

    [MaxLength(50)]
    public string? LogoContentType { get; set; }
}
