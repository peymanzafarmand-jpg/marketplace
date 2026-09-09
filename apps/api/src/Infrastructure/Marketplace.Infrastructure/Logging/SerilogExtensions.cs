using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Marketplace.Infrastructure.Logging;

/// <summary>
/// Serilog bootstrap: console + (optionally) file sink, enriched with CorrelationId and
/// standard machine/environment context. SensitiveDataDestructuringPolicy guards against
/// accidentally logging a Password/Token/Secret-named property even if a future handler
/// passes a whole request object into a log call (Architecture doc "Logging" requirement).
/// </summary>
public static class SerilogExtensions
{
    public static void ConfigureSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithProperty("Application", "Marketplace.API")
                .Destructure.With(new SensitiveDataDestructuringPolicy())
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .WriteTo.Console(outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] ({CorrelationId}) {Message:lj} {Properties:j}{NewLine}{Exception}");
        });
    }
}
