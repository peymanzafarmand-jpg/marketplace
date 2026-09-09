using System.Reflection;
using FluentValidation;
using Marketplace.Application.Common.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace Marketplace.Application;

/// <summary>
/// Application-layer composition root. Registers MediatR (scanning this assembly for every
/// module's handlers), the validation/logging pipeline, and all FluentValidation validators.
/// Called once from Marketplace.API's Program.cs.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        services.AddValidatorsFromAssembly(assembly);

        // Order matters: unhandled-exception context wraps everything, then logging,
        // then validation runs last so a validation failure never gets an "unhandled" log.
        services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));
        services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
