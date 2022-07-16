namespace Animato.Sso.WebApi.Middlewares;

using Animato.Sso.Application.Common;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate next;
    private readonly string headerName;

    public CorrelationIdMiddleware(RequestDelegate next, GlobalOptions globalOptions)
    {
        if (globalOptions == null)
        {
            throw new ArgumentNullException(nameof(globalOptions));
        }

        this.next = next ?? throw new ArgumentNullException(nameof(next));
        headerName = globalOptions.CorrelationHeaderName;
    }

    public Task Invoke(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(headerName, out var correlationId))
        {
            context.TraceIdentifier = correlationId;
        }

        context.Response.OnStarting(() =>
        {
            context.Response.Headers.Add(headerName, new[] { context.TraceIdentifier });
            return Task.CompletedTask;
        });

        return next(context);
    }
}
