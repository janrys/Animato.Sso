namespace Animato.Sso.Infrastructure.AzureStorage.Services;
using System;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Domain.Entities;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public class TableStorageHealthCheck : IHealthCheck
{
    public const string Name = "Azure table storage";
    private readonly IScopeRepository scopeRepository;

    public TableStorageHealthCheck(IScopeRepository scopeRepository) => this.scopeRepository = scopeRepository ?? throw new ArgumentNullException(nameof(scopeRepository));

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var isHealthy = true;

        try
        {
            var scope = await scopeRepository.GetScopeByName(Scope.All.Name, cancellationToken);
        }
        catch
        {
            isHealthy = false;
        }

        if (!isHealthy)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, "Azure table infrastructure failes");
        }

        return HealthCheckResult.Healthy("Azure table infrastructure is ok");
    }
}
