namespace Lavadero.Application.DTOs;

public sealed record ConfiguracionLavaderoDto(
    int Id,
    string Nombre,
    string? Telefono,
    string? Direccion,
    string? WhatsApp,
    bool TieneLogo,
    string? LogoContentType);
