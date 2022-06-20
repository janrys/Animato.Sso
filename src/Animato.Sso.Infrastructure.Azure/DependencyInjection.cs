namespace Animato.Sso.Infrastructure.AzureStorage;

using System.Reflection;
using Animato.Sso.Application.Common;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Infrastructure.AzureStorage.Services.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddAzureInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        var globalOptions = new GlobalOptions();
        configuration.Bind(GlobalOptions.ConfigurationKey, globalOptions);

        if (globalOptions.Persistence.Equals("azuretable", StringComparison.OrdinalIgnoreCase))
        {
            services.AddAzureTablePersistence(configuration);
        }

        return services;
    }

    private static IServiceCollection AddAzureTablePersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var azureTableOptions = new AzureTableStorageOptions();
        configuration.Bind(AzureTableStorageOptions.ConfigurationKey, azureTableOptions);
        services.AddSingleton(azureTableOptions);

        services.AddSingleton<AzureTableStorageDataContext>();
        services.AddSingleton<IUserRepository, AzureTableUserRepository>();
        services.AddSingleton<IApplicationRepository, AzureTableApplicationRepository>();
        services.AddSingleton<IApplicationRoleRepository, AzureTableApplicationRoleRepository>();
        services.AddSingleton<IAuthorizationCodeRepository, AzureTableAuthorizationCodeRepository>();
        services.AddSingleton<ITokenRepository, AzureTableTokenRepository>();
        services.AddSingleton<IScopeRepository, AzureTableScopeRepository>();
        services.AddSingleton<IClaimRepository, AzureTableClaimRepository>();
        return services;
    }
}
