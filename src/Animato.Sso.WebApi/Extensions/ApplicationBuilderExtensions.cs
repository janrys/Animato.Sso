namespace Animato.Sso.WebApi.Extensions;

using Hellang.Middleware.ProblemDetails;

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
}
