using Lavadero.Domain.Enums;
using Xunit;

namespace Lavadero.Tests;

public sealed class EstadoTurnoTransicionesTests
{
    [Theory]
    [InlineData(EstadoTurno.Pendiente, EstadoTurno.Confirmado)]
    [InlineData(EstadoTurno.Pendiente, EstadoTurno.Cancelado)]
    [InlineData(EstadoTurno.Confirmado, EstadoTurno.Completado)]
    [InlineData(EstadoTurno.Confirmado, EstadoTurno.Cancelado)]
    public void AceptaTransicionesPermitidas(EstadoTurno actual, EstadoTurno nuevo)
    {
        Assert.True(EstadoTurnoTransiciones.EsValida(actual, nuevo));
    }

    [Theory]
    [InlineData(EstadoTurno.Pendiente, EstadoTurno.Completado)]
    [InlineData(EstadoTurno.Completado, EstadoTurno.Pendiente)]
    [InlineData(EstadoTurno.Cancelado, EstadoTurno.Confirmado)]
    public void RechazaTransicionesInvalidas(EstadoTurno actual, EstadoTurno nuevo)
    {
        Assert.False(EstadoTurnoTransiciones.EsValida(actual, nuevo));
    }
}
