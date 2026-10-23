using System.ComponentModel.DataAnnotations;

namespace Lavadero.Application.Models;

public sealed class CrearPedidoRequest
{
    [Required, StringLength(100)] public string Nombre { get; set; } = string.Empty;
    [Required, StringLength(100)] public string Apellido { get; set; } = string.Empty;
    [Required, StringLength(30, MinimumLength = 6)] public string Celular { get; set; } = string.Empty;
    public Dictionary<int, int> Cantidades { get; set; } = new();
}
