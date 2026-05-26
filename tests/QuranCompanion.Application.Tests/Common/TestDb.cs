using Microsoft.EntityFrameworkCore;
using QuranCompanion.Infrastructure.Persistence;

namespace QuranCompanion.Application.Tests.Common;

/// <summary>
/// Spins up a fresh in-memory ApplicationDbContext per call. Each test gets its
/// own isolated database; no state leaks across tests.
/// </summary>
internal static class TestDb
{
    public static ApplicationDbContext Create(out TestClock clock)
    {
        clock = new TestClock();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"test-{Guid.NewGuid()}")
            // The provider warns when a transaction is requested but ignored.
            // Our queries never start transactions, so the warning shouldn't
            // fire, but the EF in-memory provider is noisy about a few things
            // — suppress those that don't affect query semantics.
            .ConfigureWarnings(warnings => warnings
                .Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options, clock);
    }

    public static ApplicationDbContext Create() => Create(out _);
}
