using Marketplace.API.Extensions;
using Marketplace.API.Middlewares;
using Marketplace.Application;
using Marketplace.Infrastructure;
using Marketplace.Infrastructure.Logging;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// --- Logging (must be configured before anything else logs) ---
builder.ConfigureSerilog();

// --- Layered composition roots (Architecture doc section 1) ---
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// --- API-layer concerns ---
builder.Services.AddControllers();
builder.Services.AddMarketplaceApiVersioning();
builder.Services.AddMarketplaceSwagger();
builder.Services.AddMarketplaceAuthorization();
builder.Services.AddMarketplaceHealthChecks(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Marketplace API v1"));
}

// Order matters: correlation id first (everything downstream needs it), then request
// logging, then the global exception handler (so it can log with the correlation id too).
app.UseCorrelationId();
app.UseMarketplaceRequestLogging();
app.UseGlobalExceptionHandling();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapMarketplaceHealthChecks();

Log.Information("Marketplace.API starting in {Environment} environment", app.Environment.EnvironmentName);

app.Run();

// Exposed for WebApplicationFactory<Program> in the integration test project.
public partial class Program { }
