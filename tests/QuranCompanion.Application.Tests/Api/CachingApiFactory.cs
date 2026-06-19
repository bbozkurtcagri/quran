using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using QuranCompanion.Application.Tests.Common;
using QuranCompanion.Infrastructure.Persistence;

namespace QuranCompanion.Application.Tests.Api;

/// <summary>
/// Boots the real API pipeline in-process, but swaps the Postgres-backed
/// <see cref="ApplicationDbContext"/> for an EF in-memory database that we
/// seed from <see cref="SampleData"/>. Tests can then hit the cache layer,
/// ETag middleware, and controllers exactly as in production.
/// </summary>
internal sealed class CachingApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = "cache-tests-" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Production"); // ensures CacheControl keeps 'immutable'
        builder.ConfigureAppConfiguration((_, config) =>
        {
            // The real connection string is irrelevant — we replace the DbContext below.
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = "Host=fake;Database=fake;Username=fake;Password=fake",
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // Strip the entire production EF wiring (Npgsql provider + its
            // option-configure delegate + the DbContext registration) before
            // re-adding an in-memory equivalent. EF Core throws when two
            // providers share a service collection.
            var toRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>)
                    || d.ServiceType == typeof(DbContextOptions)
                    || d.ServiceType == typeof(ApplicationDbContext)
                    || d.ServiceType == typeof(IDbContextOptionsConfiguration<ApplicationDbContext>))
                .ToList();
            foreach (var d in toRemove)
            {
                services.Remove(d);
            }

            services.AddDbContext<ApplicationDbContext>(opts =>
            {
                opts.UseInMemoryDatabase(_dbName);
                opts.ConfigureWarnings(w => w.Ignore(
                    Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
            });

            // The Npgsql health check is unusable without a real database;
            // drop it so /health/ready doesn't trip on a missing connection.
            services.RemoveAll<HealthCheckService>();
            services.AddHealthChecks();
        });
    }

    public async Task SeedAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await SampleData.SeedAsync(db);
    }
}
