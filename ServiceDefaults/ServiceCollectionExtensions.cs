using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Http.Resilience;
using OpenTelemetry.Trace;
using OpenTelemetry;                              // <-- added (core API)
using OpenTelemetry.Metrics;
namespace ServiceDefaults;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServiceDefaults(this IServiceCollection services)
    {
        // Health checks – generic ready check
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy());

        // Resilience – default Polly policies for outgoing HTTP calls
        services.AddHttpClient("ResilientClient")
            .AddStandardResilienceHandler();

        // OpenTelemetry – tracing & metrics (console exporter for demo)
        services.AddOpenTelemetry().WithTracing(builder =>
        {
            builder.AddAspNetCoreInstrumentation()
                   .AddHttpClientInstrumentation()
                   .AddConsoleExporter();
        });

        return services;
    }
}
