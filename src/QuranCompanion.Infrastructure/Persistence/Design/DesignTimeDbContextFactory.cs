using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using QuranCompanion.Application.Abstractions.Persistence;
using QuranCompanion.Infrastructure.Time;

namespace QuranCompanion.Infrastructure.Persistence.Design;

/// <summary>
/// Used by `dotnet ef` at design time only. Runtime registration lives in
/// QuranCompanion.Infrastructure.DependencyInjection.AddInfrastructure.
/// </summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("QURANCOMPANION_POSTGRES")
            ?? "Host=localhost;Port=5432;Database=qurancompanion;Username=qurancompanion;Password=qurancompanion;";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__ef_migrations_history", "public"))
            .UseSnakeCaseNamingConvention()
            .Options;

        return new ApplicationDbContext(options, new SystemClock());
    }
}
