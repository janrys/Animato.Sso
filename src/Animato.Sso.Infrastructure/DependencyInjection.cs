namespace Animato.Sso.Infrastructure;

using System.Reflection;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Infrastructure.Services;
using Animato.Sso.Infrastructure.Services.Messaging;
using Animato.Sso.Infrastructure.Services.Persistence;
using Animato.Sso.Infrastructure.Services.Totp;
using Animato.Sso.Infrastructure.Transformations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddSingleton<InMemoryDataContext>();
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();
        services.AddSingleton<IApplicationRepository, InMemoryApplicationRepository>();
        services.AddSingleton<IAuthorizationCodeRepository, InMemoryAuthorizationCodeRepository>();
        services.AddSingleton<IDataSeeder, InMemoryDataSeeder>();

        var qrCodeAuthenticatorOptions = new GoogleQrCodeTotpAuthenticatorOptions();
        configuration.Bind(GoogleQrCodeTotpAuthenticatorOptions.CONFIGURATION_KEY, qrCodeAuthenticatorOptions);
        services.AddSingleton(qrCodeAuthenticatorOptions);
        services.AddSingleton<IQrCodeTotpAuthenticator, GoogleQrCodeTotpAuthenticator>();

        services.AddTransformations(configuration);

        services.AddSingleton<IDateTimeService, DateTimeService>();
        services.AddSingleton<IDomainEventService>(service
            => new LoggingDomainEventService(new NullDomainEventService(), service.GetService<ILogger<LoggingDomainEventService>>()));

        return services;
    }

    private static void AddTransformations(this IServiceCollection services, IConfiguration configuration)
    {
        var designifyOptions = new DesignifyOptions();
        configuration.Bind(DesignifyOptions.CONFIGURATION_KEY, designifyOptions);
        services.AddSingleton(designifyOptions);
        services.AddHttpClient(DesignifyApiTransformation.HTTP_CLIENT_NAME);
        services.AddSingleton<IAssetTransformation, DesignifyApiTransformation>();
        services.AddSingleton<IAssetTransformation, WatermarkAssetTransformation>();
        services.AddSingleton<IAssetTransformation, ImageResizerAssetTransformation>();
        services.AddSingleton<IAssetTransformation, Fake02AssetTransformation>();
        services.AddSingleton<IAssetTransformation, Fake01AssetTransformation>();
        services.AddSingleton<IAssetTransformationFactory, StaticAssetTransformationFactory>();
    }
}
