using System.ComponentModel.DataAnnotations;

namespace Lavadero.Application.Models;

public sealed class CrearPedidoRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(100, ErrorMessage = "El apellido no puede superar los 100 caracteres.")]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "El celular es obligatorio.")]
    [StringLength(30, MinimumLength = 6, ErrorMessage = "El celular debe tener entre 6 y 30 caracteres.")]
    public string Celular { get; set; } = string.Empty;

    public Dictionary<int, int> Cantidades { get; set; } = new();
}
