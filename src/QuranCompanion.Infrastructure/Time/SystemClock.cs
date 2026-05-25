using QuranCompanion.Application.Abstractions.Persistence;

namespace QuranCompanion.Infrastructure.Time;

internal sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
