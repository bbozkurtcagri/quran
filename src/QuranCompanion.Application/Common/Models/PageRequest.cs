namespace QuranCompanion.Application.Common.Models;

public sealed record PageRequest(int Page, int PageSize)
{
    public const int DefaultPage = 1;

    public const int DefaultPageSize = 20;

    // 300 accommodates a full surah in one request (longest = Al-Baqarah, 286 verses)
    // while still capping abuse from arbitrary clients.
    public const int MaxPageSize = 300;

    public static PageRequest Default => new(DefaultPage, DefaultPageSize);

    public int Offset => (Page - 1) * PageSize;
}
