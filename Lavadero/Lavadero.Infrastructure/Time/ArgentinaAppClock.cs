using Lavadero.Application.Interfaces;

namespace Lavadero.Infrastructure.Time;

public sealed class ArgentinaAppClock(TimeProvider timeProvider) : IAppClock
{
    private static readonly TimeZoneInfo ArgentinaTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");

    public DateTime UtcNow => timeProvider.GetUtcNow().UtcDateTime;

    public DateTime LocalNow =>
        TimeZoneInfo.ConvertTimeFromUtc(UtcNow, ArgentinaTimeZone);

    public DateTime LocalToday => LocalNow.Date;

    public DateTime ToUtc(DateTime localDateTime) =>
        TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified),
            ArgentinaTimeZone);
}
