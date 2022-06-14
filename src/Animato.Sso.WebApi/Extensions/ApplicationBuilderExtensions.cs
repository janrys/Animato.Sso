namespace Animato.Sso.WebApi.Extensions;

using Hellang.Middleware.ProblemDetails;
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

    public static IApplicationBuilder UseCustomLogging(this IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging();
        return app;
    }

    public static ILogger CreateLogger()
        => new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .WriteTo.Async(a => a.Console())
            .CreateBootstrapLogger();
}
