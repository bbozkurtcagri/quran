using QuranCompanion.Application.Abstractions.Persistence;

namespace QuranCompanion.Application.Tests.Common;

internal sealed class TestClock(DateTime? fixedUtcNow = null) : IClock
{
    public DateTime UtcNow { get; } = fixedUtcNow ?? new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
}
