namespace Animato.Sso.Infrastructure.Services.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Domain.Entities;
using Microsoft.Extensions.Logging;

public class InMemoryApplicationRepository : IApplicationRepository
{
    private const string ERROR_LOADING_APPLICATIONS = "Error loading applications";
    private const string ERROR_LOADING_ROLES = "Error loading roles";
    private readonly InMemoryDataContext dataContext;
    private readonly ILogger<InMemoryApplicationRepository> logger;

    public InMemoryApplicationRepository(InMemoryDataContext dataContext, ILogger<InMemoryApplicationRepository> logger)
    {
        this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<Application> GetByCode(string code, CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(dataContext.Applications.FirstOrDefault(u => u.Code.Equals(code, StringComparison.OrdinalIgnoreCase)));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_LOADING_APPLICATIONS);
            throw;
        }
    }

    public Task<Application> GetById(Domain.Entities.ApplicationId applicationId, CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(dataContext.Applications.FirstOrDefault(u => u.Id == applicationId));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_LOADING_APPLICATIONS);
            throw;
        }
    }

    public Task<IEnumerable<ApplicationRole>> GetRoles(Domain.Entities.ApplicationId applicationId, CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(dataContext.ApplicationRoles.Where(r => r.ApplicationId == applicationId));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_LOADING_ROLES);
            throw;
        }
    }

    public Task<IEnumerable<ApplicationRole>> GetUserRoles(Domain.Entities.ApplicationId applicationId, UserId userId, CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(dataContext
                .ApplicationRoles.Where(r => r.ApplicationId == applicationId)
                .Join(dataContext.UserApplicationRoles
                .Where(uar => uar.UserId == userId), ar => ar.Id, uar => uar.ApplicationRoleId, (ar, uar) => ar));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_LOADING_ROLES);
            throw;
        }
    }
}
