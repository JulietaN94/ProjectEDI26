using Lavadero.Domain.Enums;

namespace Lavadero.Application.DTOs;

public sealed record TurnoDto(
    int Id,
    DateTime FechaHora,
    EstadoTurno Estado,
    string? Observaciones,
    DateTime FechaCreacion,
    int ClienteId,
    string ClienteNombre,
    string ClienteTelefono,
    int AutoId,
    string AutoMarca,
    string AutoModelo,
    int ServicioId,
    string ServicioNombre,
    decimal ServicioPrecio,
    int ServicioDuracionMinutos);
