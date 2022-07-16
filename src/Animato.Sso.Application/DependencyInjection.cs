namespace Animato.Sso.Application;
using System.Reflection;
using Animato.Sso.Application.Common;
using Animato.Sso.Application.Common.Behaviours;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Security;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        var globalOptions = new GlobalOptions();
        configuration.Bind(GlobalOptions.ConfigurationKey, globalOptions);
        services.AddSingleton(globalOptions);

        var oidcOptions = new OidcOptions();
        configuration.Bind(OidcOptions.ConfigurationKey, oidcOptions);
        services.AddSingleton(oidcOptions);

        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(Assembly.GetExecutingAssembly());
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
        services.AddSingleton<IAuthorizationService, StaticMapAuthorizationService>();
        services.AddSingleton<IPasswordFactory, PasswordFactory>();
        services.AddSingleton<IClaimFactory, ClaimFactory>();
        services.AddSingleton<ITokenFactory, TokenFactory>();
        services.AddSingleton<ICertificateManager, LocalFileCertificateManager>();

        return services;
    }
}
