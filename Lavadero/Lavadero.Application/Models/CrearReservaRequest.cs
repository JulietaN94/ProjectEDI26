using System.ComponentModel.DataAnnotations;

namespace Lavadero.Application.Models;

/// <summary>
/// Datos mínimos solicitados al cliente para reservar un turno.
/// </summary>
public sealed class CrearReservaRequest
{
    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [MaxLength(30, ErrorMessage = "El teléfono no puede superar los 30 caracteres.")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "La marca del auto es obligatoria.")]
    [MaxLength(50, ErrorMessage = "La marca no puede superar los 50 caracteres.")]
    public string Marca { get; set; } = string.Empty;

    [Required(ErrorMessage = "El modelo del auto es obligatorio.")]
    [MaxLength(50, ErrorMessage = "El modelo no puede superar los 50 caracteres.")]
    public string Modelo { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un servicio.")]
    public int ServicioId { get; set; }

    [Required(ErrorMessage = "Debe seleccionar una fecha y hora.")]
    public DateTime FechaHora { get; set; }

    [MaxLength(500, ErrorMessage = "Las observaciones no pueden superar los 500 caracteres.")]
    public string? Observaciones { get; set; }
}
