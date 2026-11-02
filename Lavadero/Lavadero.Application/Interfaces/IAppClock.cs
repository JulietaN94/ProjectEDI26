namespace Lavadero.Application.Interfaces;

public interface IAppClock
{
    DateTime UtcNow { get; }
    DateTime LocalNow { get; }
    DateTime LocalToday { get; }
    DateTime ToUtc(DateTime localDateTime);
}
