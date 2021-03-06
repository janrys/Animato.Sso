namespace Animato.Sso.Infrastructure;

using System.Reflection;
using Animato.Sso.Application.Common;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Infrastructure.Services;
using Animato.Sso.Infrastructure.Services.Messaging;
using Animato.Sso.Infrastructure.Services.Persistence;
using Animato.Sso.Infrastructure.Services.Totp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        var globalOptions = new GlobalOptions();
        configuration.Bind(GlobalOptions.ConfigurationKey, globalOptions);

        if (globalOptions.Persistence.Equals("inmemory", StringComparison.OrdinalIgnoreCase))
        {
            services.AddInMemoryPersistence();
        }
        services.AddSingleton<IDataSeeder, DataSeeder>();

        var qrCodeAuthenticatorOptions = new GoogleQrCodeTotpAuthenticatorOptions();
        configuration.Bind(GoogleQrCodeTotpAuthenticatorOptions.CONFIGURATION_KEY, qrCodeAuthenticatorOptions);
        services.AddSingleton(qrCodeAuthenticatorOptions);
        services.AddSingleton<IQrCodeTotpAuthenticator, GoogleQrCodeTotpAuthenticator>();
        services.AddSingleton<IDateTimeService, DateTimeService>();
        services.AddSingleton<IDomainEventService>(service
            => new LoggingDomainEventService(new NullDomainEventService(), service.GetService<ILogger<LoggingDomainEventService>>()));

        return services;
    }

    private static IServiceCollection AddInMemoryPersistence(this IServiceCollection services)
    {
        services.AddSingleton<InMemoryDataContext>();
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();
        services.AddSingleton<IApplicationRepository, InMemoryApplicationRepository>();
        services.AddSingleton<IApplicationRoleRepository, InMemoryApplicationRoleRepository>();
        services.AddSingleton<IAuthorizationCodeRepository, InMemoryAuthorizationCodeRepository>();
        services.AddSingleton<ITokenRepository, InMemoryTokenRepository>();
        services.AddSingleton<IScopeRepository, InMemoryScopeRepository>();
        services.AddSingleton<IClaimRepository, InMemoryClaimRepository>();
        return services;
    }
}
