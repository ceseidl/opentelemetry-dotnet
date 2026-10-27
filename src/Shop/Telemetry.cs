using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Shop;

public static class Telemetry
{
    // One call per service: the three signals, the same
    // resource and one exporter (OTLP) for all of them.
    public static void AddShopTelemetry(
        this WebApplicationBuilder builder,
        string name, string version)
    {
        builder.Logging.ClearProviders();

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(
                serviceName: name,
                serviceVersion: version))
            .WithTracing(t => t
                .AddAspNetCoreInstrumentation(o =>
                    o.Filter = ctx =>
                        ctx.Request.Path != "/health")
                .AddHttpClientInstrumentation()
                .AddSource(Payments.Source.Name))
            .WithMetrics(m => m
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddMeter(Payments.Meter.Name))
            .WithLogging(
                configureBuilder: null,
                configureOptions: o =>
                    o.IncludeFormattedMessage = true)
            .UseOtlpExporter();
    }
}
