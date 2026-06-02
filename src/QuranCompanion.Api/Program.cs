using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using QuranCompanion.Api.Common;
using QuranCompanion.Api.Seeding;
using QuranCompanion.Application;
using QuranCompanion.Infrastructure;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName());

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<SeedRunner>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Route framework-level model binding/validation failures through our
// ApiResponse envelope so clients always get the same shape on 400.
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(kvp => kvp.Value?.Errors.Count > 0)
            .SelectMany(kvp => kvp.Value!.Errors
                .Select(e => new ApiError(
                    "validation_error",
                    string.IsNullOrEmpty(e.ErrorMessage)
                        ? $"The {kvp.Key} field is invalid."
                        : e.ErrorMessage,
                    kvp.Key)))
            .ToList();

        return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(
            ApiResponse<object>.Fail("Validation failed", errors));
    };
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddOutputCache(options =>
{
    // Immutable Quran data — cache aggressively server-side. Cache key
    // includes the path and query string by default.
    options.AddPolicy("Immutable", policy => policy
        .Expire(TimeSpan.FromHours(24))
        .SetVaryByQuery("translationSourceCode", "page", "pageSize")
        .Tag("quran"));

    // Translation sources: short TTL so admin changes propagate quickly.
    options.AddPolicy("TranslationSources", policy => policy
        .Expire(TimeSpan.FromMinutes(5))
        .SetVaryByQuery("languageCode")
        .Tag("translation-sources"));
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            }));
});

builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured."),
        name: "postgres",
        tags: ["db", "ready"]);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(_ => true);
    });
});

var app = builder.Build();

if (args.Length > 0 && string.Equals(args[0], "seed", StringComparison.OrdinalIgnoreCase))
{
    var seedDirectory = args.Length > 1
        ? Path.GetFullPath(args[1])
        : ResolveSeedDirectory();

    using var scope = app.Services.CreateScope();
    var seedRunner = scope.ServiceProvider.GetRequiredService<SeedRunner>();
    var seedLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    seedLogger.LogInformation("Seeding from {Directory}…", seedDirectory);
    await seedRunner.RunAsync(seedDirectory, CancellationToken.None);
    seedLogger.LogInformation("Seed completed.");
    return;
}

static string ResolveSeedDirectory()
{
    // Walk up from the CWD looking for a seed-data/ folder so the CLI works
    // whether invoked from the repo root or from inside the project directory
    // (which is what `dotnet run --project …` does).
    var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (dir is not null)
    {
        var candidate = Path.Combine(dir.FullName, "seed-data");
        if (Directory.Exists(candidate))
        {
            return candidate;
        }
        dir = dir.Parent;
    }
    return Path.Combine(Directory.GetCurrentDirectory(), "seed-data");
}

app.UseForwardedHeaders();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("QuranCompanion API")
            .WithTheme(ScalarTheme.Default)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseSerilogRequestLogging();

// Swallow client-cancellation exceptions so Serilog sees a normal 499 return
// rather than an unhandled exception. Sits inside Serilog (so 499 is logged)
// but outside the rest, since UseExceptionHandler re-throws cancellation
// exceptions and would bypass any IExceptionHandler we register.
app.UseMiddleware<ClientCancellationMiddleware>();

app.UseCors();
app.UseRateLimiter();

// OutputCache + ETag middleware are temporarily disabled. They cause empty
// responses to be served to concurrent clients when one of them aborts
// mid-flight (single-flight coalescing returns the cancelled response to
// waiters). Cache-Control headers continue to let the browser cache freely.
// Re-enable after splitting the policy to opt out of request coalescing
// or porting ETag middleware to IHttpResponseBodyFeature.
// app.UseOutputCache();
// app.UseMiddleware<ETagMiddleware>();

app.MapControllers();

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready", new()
{
    Predicate = check => check.Tags.Contains("ready"),
});

app.Run();
