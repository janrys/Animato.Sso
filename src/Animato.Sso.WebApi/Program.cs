using Animato.Sso.Application;
using Animato.Sso.Infrastructure;
using Animato.Sso.Infrastructure.AzureStorage;
using Animato.Sso.WebApi.Extensions;
using Serilog;

Log.Logger = Animato.Sso.WebApi.Extensions.ApplicationBuilderExtensions.CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Configuration.AddCustomConfiguration(builder.Environment.EnvironmentName);
    builder.AddCustomLogging();
    builder.Services.AddApplication(builder.Configuration);
    builder.Services
        .AddInfrastructure(builder.Configuration)
        .AddAzureInfrastructure(builder.Configuration);
    builder.Services.AddWebApi(builder.Environment);

    var app = builder.Build();
    //if (app.Environment.IsDevelopment())
    if (true)
    {
        app.UseCustomSwagger();
        app.UseDeveloperExceptionPage();
    }

    app.UseCustomLogging();
    app.UseCustomProblemDetails();
    app.UseHealthChecks("/health");
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    var cookiePolicyOptions = new CookiePolicyOptions
    {
        MinimumSameSitePolicy = SameSiteMode.Lax,
    };
    app.UseCookiePolicy(cookiePolicyOptions);
    app.MapControllers();

    app.Run();
    return 0;
}
catch (Exception exception)
{
    Log.Fatal(exception, "Host terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}


