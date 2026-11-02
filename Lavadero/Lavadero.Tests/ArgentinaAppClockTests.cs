using Lavadero.Infrastructure.Time;
using Xunit;

namespace Lavadero.Tests;

public sealed class ArgentinaAppClockTests
{
    [Fact]
    public void ConvierteUtcAlDiaLocalDeArgentina()
    {
        var utc = new DateTimeOffset(2026, 11, 2, 2, 30, 0, TimeSpan.Zero);
        var clock = new ArgentinaAppClock(new FixedTimeProvider(utc));

        Assert.Equal(new DateTime(2026, 11, 1, 23, 30, 0), clock.LocalNow);
        Assert.Equal(new DateTime(2026, 11, 1), clock.LocalToday);
    }

    [Fact]
    public void ConvierteMedianocheArgentinaAUtc()
    {
        var clock = new ArgentinaAppClock(
            new FixedTimeProvider(new DateTimeOffset(2026, 11, 2, 12, 0, 0, TimeSpan.Zero)));

        var utc = clock.ToUtc(new DateTime(2026, 11, 2));

        Assert.Equal(new DateTime(2026, 11, 2, 3, 0, 0, DateTimeKind.Utc), utc);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
