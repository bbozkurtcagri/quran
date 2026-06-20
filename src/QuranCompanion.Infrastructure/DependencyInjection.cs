using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuranCompanion.Application.Abstractions.Embedding;
using QuranCompanion.Application.Abstractions.Persistence;
using QuranCompanion.Infrastructure.Embedding;
using QuranCompanion.Infrastructure.Persistence;
using QuranCompanion.Infrastructure.Time;

namespace QuranCompanion.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException(
                "Connection string 'Postgres' is not configured.");

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsHistoryTable("__ef_migrations_history", "public");
                npgsql.EnableRetryOnFailure(3);
                npgsql.UseVector(); // register pgvector mapping
            });
            options.UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());

        services.AddSingleton<IClock, SystemClock>();

        services
            .AddOptions<EmbedderOptions>()
            .Bind(configuration.GetSection(EmbedderOptions.SectionName))
            .Validate(
                o => Uri.TryCreate(o.BaseUrl, UriKind.Absolute, out _),
                $"{EmbedderOptions.SectionName}.BaseUrl must be an absolute URL.");

        services
            .AddHttpClient<IEmbedder, HttpEmbedder>((sp, client) =>
            {
                var opts = sp.GetRequiredService<
                    Microsoft.Extensions.Options.IOptions<EmbedderOptions>>().Value;
                client.BaseAddress = new Uri(opts.BaseUrl);
                client.Timeout = opts.Timeout;
            });

        return services;
    }
}
