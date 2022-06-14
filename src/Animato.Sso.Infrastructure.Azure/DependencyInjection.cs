namespace Animato.Sso.Infrastructure.Azure;

using System.Reflection;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Infrastructure.Azure.Services.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        var azureTableOptions = new AzureTableStorageOptions();
        configuration.Bind(AzureTableStorageOptions.ConfigurationKey, azureTableOptions);
        services.AddSingleton(azureTableOptions);

        if (configuration["Database"].Equals("azuretable", StringComparison.OrdinalIgnoreCase))
        {
            services.AddAzureTablePersistence();
        }

        return services;
    }

    private static IServiceCollection AddAzureTablePersistence(this IServiceCollection services)
    {
        services.AddSingleton<AzureTableStorageDataContext>();
        services.AddSingleton<IUserRepository, AzureTableUserRepository>();
        services.AddSingleton<IApplicationRepository, AzureTableApplicationRepository>();
        services.AddSingleton<IApplicationRoleRepository, AzureTableApplicationRoleRepository>();
        services.AddSingleton<IAuthorizationCodeRepository, AzureTableAuthorizationCodeRepository>();
        services.AddSingleton<ITokenRepository, AzureTableTokenRepository>();
        services.AddSingleton<IDataSeeder, AzureTableDataSeeder>();
        return services;
    }
}
