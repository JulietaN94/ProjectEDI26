namespace Lavadero.Application.DTOs;

public sealed record ServicioDto(
    int Id,
    string Nombre,
    string? Descripcion,
    decimal Precio,
    int DuracionMinutos,
    bool Activo);
