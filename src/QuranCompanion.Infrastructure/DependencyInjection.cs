using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
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

        // pgvector type mapping'i iki katmanda register etmek gerek:
        // 1) NpgsqlDataSource seviyesinde (UseVector çağrısı) — raw parameter
        //    passing için (SearchVerses semantic query'de "WHERE embedding <=> @vec").
        //    Bu olmadan NpgsqlParameter type'ı çözemez, InvalidCastException atar.
        // 2) EF Core seviyesinde (options.UseVector) — LINQ expression translation
        //    ve column mapping için.
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.UseVector();
        var dataSource = dataSourceBuilder.Build();

        services.AddSingleton(dataSource);

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseNpgsql(dataSource, npgsql =>
            {
                npgsql.MigrationsHistoryTable("__ef_migrations_history", "public");
                npgsql.EnableRetryOnFailure(3);
                npgsql.UseVector();
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
