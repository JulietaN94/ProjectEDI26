namespace Lavadero.Application.DTOs;

public sealed record ClienteDto(
    int Id,
    string NombreCompleto,
    string Telefono,
    DateTime FechaAlta);
