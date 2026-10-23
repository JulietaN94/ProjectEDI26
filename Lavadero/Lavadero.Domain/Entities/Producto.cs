using System.ComponentModel.DataAnnotations;

namespace Lavadero.Domain.Entities;

public class Producto
{
    public int Id { get; set; }
    [Required, MaxLength(50)] public string Codigo { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Nombre { get; set; } = string.Empty;
    [MaxLength(500)] public string? Descripcion { get; set; }
    [MaxLength(100)] public string? Marca { get; set; }
    public decimal PrecioVenta { get; set; }
    public byte[]? Imagen { get; set; }
    [MaxLength(50)] public string? ImagenContentType { get; set; }
    public bool Activo { get; set; } = true;
}
