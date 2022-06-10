namespace Animato.Sso.Infrastructure.Services.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Domain.Entities;
using Microsoft.Extensions.Logging;

public class InMemoryApplicationRepository : IApplicationRepository
{
    private const string ERROR_LOADING_APPLICATIONS = "Error loading applications";
    private const string ERROR_INSERTING_APPLICATIONS = "Error inserting applications";
    private const string ERROR_UPDATING_APPLICATIONS = "Error updating applications";
    private const string ERROR_DELETING_APPLICATIONS = "Error deleting applications";
    private const string ERROR_LOADING_ROLES = "Error loading roles";
    private readonly InMemoryDataContext dataContext;
    private readonly ILogger<InMemoryApplicationRepository> logger;

    public InMemoryApplicationRepository(InMemoryDataContext dataContext, ILogger<InMemoryApplicationRepository> logger)
    {
        this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<IEnumerable<Application>> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(dataContext.Applications.AsEnumerable());
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_LOADING_APPLICATIONS);
            throw;
        }
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

    public Task<Application> Create(Application application, CancellationToken cancellationToken)
    {
        try
        {
            application.Id = Domain.Entities.ApplicationId.New();
            dataContext.Applications.Add(application);
            return Task.FromResult(application);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_DELETING_APPLICATIONS);
            throw;
        }
    }

    public Task<Application> Update(Application application, CancellationToken cancellationToken)
    {
        try
        {
            var storedApplication = dataContext.Applications.FirstOrDefault(a => a.Id == application.Id);

            if (storedApplication == null)
            {
                throw new NotFoundException(nameof(Application), application.Id);
            }

            dataContext.Applications.Remove(storedApplication);
            dataContext.Applications.Add(application);

            return Task.FromResult(application);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_DELETING_APPLICATIONS);
            throw;
        }
    }

    public Task Delete(Domain.Entities.ApplicationId applicationId, CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(dataContext.Applications.RemoveAll(a => a.Id == applicationId));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_DELETING_APPLICATIONS);
            throw;
        }
    }
}
