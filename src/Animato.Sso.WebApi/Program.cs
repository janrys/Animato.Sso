using Animato.Sso.Application;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Infrastructure;
using Animato.Sso.WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddCustomConfiguration(builder.Environment.EnvironmentName);
builder.Logging.AddCustomLogging();
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddWebApi(builder.Environment);


var app = builder.Build();
//if (app.Environment.IsDevelopment())
if (true)
{
    app.UseCustomSwagger();
    app.UseDeveloperExceptionPage();
}

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

app.Services.GetService<IMetadataStorageService>().Seed();

app.Run();


