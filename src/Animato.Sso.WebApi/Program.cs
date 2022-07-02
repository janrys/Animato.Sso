using Animato.Sso.Application;
using Animato.Sso.Infrastructure;
using Animato.Sso.Infrastructure.AzureStorage;
using Animato.Sso.WebApi.Extensions;
using Serilog;

Log.Logger = Animato.Sso.WebApi.Extensions.ServiceCollectionExtensions.CreateBootstrapLogger();
var assemblyName = System.Reflection.Assembly.GetEntryAssembly().GetName();
Log.Information("Starting up {Application} {Version}", assemblyName.Name, assemblyName.Version);

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Configuration.AddCustomConfiguration(builder.Environment.EnvironmentName);
    builder.AddCustomLogging(builder.Configuration);
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
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    var cookiePolicyOptions = new CookiePolicyOptions
    {
        MinimumSameSitePolicy = SameSiteMode.Lax,
    };
    app.UseCookiePolicy(cookiePolicyOptions);
    app.MapControllers();
    app.UseCustomHealthChecks();

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
    Log.Information("Shut down complete {Application} {Version}", assemblyName.Name, assemblyName.Version);
    Log.CloseAndFlush();
}


