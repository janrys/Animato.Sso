namespace Animato.Sso.WebApi.Extensions;

using Animato.Sso.Application.Common;
using HealthChecks.UI.Client;
using Hellang.Middleware.ProblemDetails;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Extensions;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseCustomProblemDetails(this IApplicationBuilder app)
    {
        app.UseProblemDetails();
        return app;
    }

    public static IApplicationBuilder UseCustomSwagger(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        return app;
    }

    public static IApplicationBuilder UseCustomLogging(this IApplicationBuilder app, IConfiguration configuration)
    {
        var globalOptions = new GlobalOptions();
        configuration.Bind(GlobalOptions.ConfigurationKey, globalOptions);

        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RequestUrl", httpContext.Request.GetDisplayUrl());
                    diagnosticContext.Set("ClientIp", httpContext.Connection.RemoteIpAddress);
                    diagnosticContext.Set("CorrelationId", httpContext.TraceIdentifier);
                };
            options.MessageTemplate += ", url {RequestUrl}, client ip {ClientIp}, correlation id {CorrelationId}";
        });
        return app;
    }

    public static WebApplicationBuilder AddCustomLogging(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        var globalOptions = new GlobalOptions();
        configuration.Bind(GlobalOptions.ConfigurationKey, globalOptions);

        if (globalOptions.UseApplicationInsights())
        {
            builder.Services.AddApplicationInsightsTelemetry(globalOptions.ApplicationInsightsKey);
        }

        var logLevel = LogEventLevel.Information;
        if (Enum.TryParse<LogEventLevel>(globalOptions.LogLevel, true, out var parsedLogLevel))
        {
            logLevel = parsedLogLevel;
        }

        var msLogLevel = LogEventLevel.Warning;

        if (logLevel is LogEventLevel.Error or LogEventLevel.Fatal)
        {
            msLogLevel = logLevel;
        }

        builder.Logging.ClearProviders();
        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .MinimumLevel.Override("Microsoft", msLogLevel)
                    .MinimumLevel.Is(logLevel)
                    .Enrich.FromLogContext()
                    .Enrich.WithExceptionDetails()
                    .Enrich.WithMachineName()
                    .WriteTo.Async(a => a.Console());

            if (globalOptions.UseApplicationInsights())
            {
                configuration.WriteTo.ApplicationInsights(services.GetRequiredService<TelemetryConfiguration>(), TelemetryConverter.Traces);
            }

            if (globalOptions.LogToAzureDiagnosticsStream)
            {
                configuration.WriteTo.File(
               Path.Combine(Environment.GetEnvironmentVariable("HOME"), "LogFiles", "Application", "diagnostics.txt"),
               rollingInterval: RollingInterval.Day,
               fileSizeLimitBytes: 10 * 1024 * 1024,
               retainedFileCountLimit: 2,
               rollOnFileSizeLimit: true,
               shared: true,
               flushToDiskInterval: TimeSpan.FromSeconds(1));
            }
        });




        return builder;
    }

    public static IApplicationBuilder UseCustomHealthChecks(this IApplicationBuilder app)
    {
        const string healthCheckPath = "/api/health";
        app.UseHealthChecks(healthCheckPath, new HealthCheckOptions()
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        (app as WebApplication)?.MapHealthChecksUI();

        return app;
    }
}
