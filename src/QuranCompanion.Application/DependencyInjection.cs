using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using QuranCompanion.Application.Abstractions.Text;
using QuranCompanion.Application.Common.Behaviors;
using QuranCompanion.Application.Common.Text;

namespace QuranCompanion.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        services.AddSingleton<ITextNormalizer, DefaultTextNormalizer>();

        return services;
    }
}
