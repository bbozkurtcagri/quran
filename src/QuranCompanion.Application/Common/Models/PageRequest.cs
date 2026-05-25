namespace QuranCompanion.Application.Common.Models;

public sealed record PageRequest(int Page, int PageSize)
{
    public const int DefaultPage = 1;

    public const int DefaultPageSize = 20;

    public const int MaxPageSize = 100;

    public static PageRequest Default => new(DefaultPage, DefaultPageSize);

    public int Offset => (Page - 1) * PageSize;
}
