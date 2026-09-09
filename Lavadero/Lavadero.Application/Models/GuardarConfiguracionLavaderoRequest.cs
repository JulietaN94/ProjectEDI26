using System.ComponentModel.DataAnnotations;

namespace Lavadero.Application.Models;

public sealed class GuardarConfiguracionLavaderoRequest
{
    [Required(ErrorMessage = "El nombre del lavadero es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(30, ErrorMessage = "El teléfono no puede superar los 30 caracteres.")]
    [RegularExpression(
        @"^[0-9+\-\s()]+$",
        ErrorMessage = "El teléfono solo puede contener números, espacios y los símbolos +, -, ( y ).")]
    public string? Telefono { get; set; }

    [MaxLength(200, ErrorMessage = "La dirección no puede superar los 200 caracteres.")]
    public string? Direccion { get; set; }

    [MaxLength(30, ErrorMessage = "El WhatsApp no puede superar los 30 caracteres.")]
    [RegularExpression(
        @"^[0-9+\-\s()]+$",
        ErrorMessage = "El WhatsApp solo puede contener números, espacios y los símbolos +, -, ( y ).")]
    public string? WhatsApp { get; set; }

    /// <summary>
    /// Nueva imagen a guardar. Si es null se conserva la imagen actual.
    /// </summary>
    public string? NuevoLogoBase64 { get; set; }

    [MaxLength(50)]
    public string? NuevoLogoContentType { get; set; }

    /// <summary>
    /// Cuando es true elimina la imagen existente.
    /// </summary>
    public bool QuitarLogo { get; set; }
}
