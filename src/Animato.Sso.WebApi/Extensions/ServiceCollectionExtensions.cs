namespace Animato.Sso.WebApi.Extensions;

using System.Reflection;
using System.Text.Json.Serialization;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.WebApi.Filters;
using Animato.Sso.WebApi.Services;
using FluentValidation.AspNetCore;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

public static class ServiceCollectionExtensions
{
    public static IConfigurationBuilder AddCustomConfiguration(this IConfigurationBuilder builder, string environmentName)
        => builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

    public static ILoggingBuilder AddCustomLogging(this ILoggingBuilder builder)
        => builder.ClearProviders().AddConsole();

    public static IServiceCollection AddCustomProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.IncludeExceptionDetails = (ctx, ex) =>
            {
                var env = ctx.RequestServices.GetRequiredService<IHostEnvironment>();
                return env.IsDevelopment() || env.IsStaging();
            };

            options.MapFluentValidationException();
            options.MapOperationCanceledException();
            options.MapCustomExceptions();
        });

        return services;
    }

    public static IServiceCollection AddCustomSwaggerGen(this IServiceCollection services, bool includeXmlComments = true)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "Asset API",
                Description = "An ASP.NET Core Web API for managing assets",
                TermsOfService = new Uri("https://www.animato.cz/terms"),
                Contact = new OpenApiContact
                {
                    Name = "Animato",
                    Email = "helpdesk@animato.cz",
                    Url = new Uri("https://www.animato.cz/")
                }
            });

            options.EnableAnnotations();

            options.SchemaFilter<CamelCasingPropertiesFilter>();
            options.ParameterFilter<LogoTypeParameterFilter>();

            if (includeXmlComments)
            {
                options.IncludeXmlComments(GetXmlCommentPath(Assembly.GetEntryAssembly()));
                options.IncludeXmlComments(GetXmlCommentPath(typeof(Application.DependencyInjection).Assembly));
                options.IncludeXmlComments(GetXmlCommentPath(typeof(Domain.Common.DomainEvent).Assembly));
            }
        });

        return services;
    }

    private static string GetXmlCommentPath(Assembly assembly)
    {
        var xmlFile = $"{assembly?.GetName().Name}.xml";

        if (string.IsNullOrEmpty(xmlFile))
        {
            xmlFile = Path.Combine(assembly?.Location ?? "", xmlFile);
            if (!File.Exists(xmlFile))
            {
                throw new FileNotFoundException("The API file documentation is not found: " + xmlFile);
            }
        }

        return Path.Combine(AppContext.BaseDirectory, xmlFile);
    }

    public static IServiceCollection AddWebApi(this IServiceCollection services, IWebHostEnvironment environment)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<ICurrentUserService, HttpContextCurrentUserService>();

        if (environment.IsDevelopment())
        {
            services.AddDatabaseDeveloperPageExceptionFilter();
        }

        services.AddHealthChecks();

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
                options.AccessDeniedPath = "/authorize/";
            });

        services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
        services.AddControllers()
            .AddFluentValidation()
            .AddJsonOptions(options => options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);

        services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);

        services.AddCustomSwaggerGen();
        services.AddCustomProblemDetails();

        return services;
    }
}
