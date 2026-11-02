namespace Lavadero.Application.DTOs;

public sealed record DashboardAdminDto(
    int TurnosHoy,
    int PendientesHoy,
    int ConfirmadosHoy,
    int CompletadosHoy,
    decimal IngresosHoy,
    IReadOnlyList<TurnoDto> ProximosTurnos,
    int PedidosHoy,
    int PedidosPendientes,
    IReadOnlyList<PedidoDto> PedidosRecientes);
