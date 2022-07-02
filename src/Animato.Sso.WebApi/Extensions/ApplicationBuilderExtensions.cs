namespace Animato.Sso.WebApi.Extensions;

using HealthChecks.UI.Client;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Extensions;
using Serilog;

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

    public static IApplicationBuilder UseCustomLogging(this IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging(options =>
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestUrl", httpContext.Request.GetDisplayUrl());
                diagnosticContext.Set("ClientIp", httpContext.Connection.RemoteIpAddress);
            });
        return app;
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
