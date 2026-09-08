using System.ComponentModel.DataAnnotations;

namespace Lavadero.Application.Models;

/// <summary>
/// Modelo utilizado para crear o editar un servicio ofrecido por el lavadero.
/// </summary>
public sealed class GuardarServicioRequest
{
    [Required(ErrorMessage = "El nombre del servicio es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(300, ErrorMessage = "La descripción no puede superar los 300 caracteres.")]
    public string? Descripcion { get; set; }

    [Range(typeof(decimal), "0", "999999999.99", ErrorMessage = "El precio debe ser mayor o igual a cero.")]
    public decimal Precio { get; set; }

    [Range(1, 480, ErrorMessage = "La duración debe estar entre 1 y 480 minutos.")]
    public int DuracionMinutos { get; set; }

    public bool Activo { get; set; } = true;
}
