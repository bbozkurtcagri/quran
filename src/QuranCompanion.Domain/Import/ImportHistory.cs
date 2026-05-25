using QuranCompanion.Domain.Common;

namespace QuranCompanion.Domain.Import;

public class ImportHistory : BaseEntity
{
    public string SourceCode { get; set; } = null!;

    public string ImportType { get; set; } = null!;

    public string FileName { get; set; } = null!;

    public string ContentHash { get; set; } = null!;

    public ImportStatus Status { get; set; }

    public int InsertedCount { get; set; }

    public int UpdatedCount { get; set; }

    public int FailedCount { get; set; }

    public DateTime? StartedOnUtc { get; set; }

    public DateTime? CompletedOnUtc { get; set; }

    public string? ErrorMessage { get; set; }
}
