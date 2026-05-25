namespace QuranCompanion.Application.Abstractions.Persistence;

public interface IClock
{
    DateTime UtcNow { get; }
}
