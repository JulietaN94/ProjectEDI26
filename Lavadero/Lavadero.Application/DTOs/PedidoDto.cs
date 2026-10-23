using Lavadero.Domain.Entities;

namespace Lavadero.Application.DTOs;

public sealed record PedidoDto(int Id, string Nombre, string Apellido, string Celular,
    DateTime FechaUtc, EstadoPedido Estado, IReadOnlyList<DetallePedidoDto> Detalles);

public sealed record DetallePedidoDto(string Codigo, string Nombre, decimal PrecioUnitario, int Cantidad);
