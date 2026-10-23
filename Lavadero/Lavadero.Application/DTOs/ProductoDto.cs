namespace Lavadero.Application.DTOs;

public sealed record ProductoDto(int Id, string Codigo, string Nombre, string? Descripcion,
    string? Marca, decimal PrecioVenta, byte[]? Imagen, string? ImagenContentType, bool Activo);
