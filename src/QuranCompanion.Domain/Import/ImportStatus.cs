namespace QuranCompanion.Domain.Import;

public enum ImportStatus
{
    Pending = 0,
    Running = 1,
    Succeeded = 2,
    Failed = 3,
    SkippedNoChange = 4,
}
