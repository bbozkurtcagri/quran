namespace QuranCompanion.Domain.Common;

public abstract class BaseEntity
{
    public long Id { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime? UpdatedOnUtc { get; set; }

    public bool IsDeleted { get; set; }
}
