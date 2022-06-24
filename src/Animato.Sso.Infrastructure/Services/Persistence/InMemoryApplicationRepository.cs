namespace Animato.Sso.Infrastructure.Services.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Common.Logging;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Domain.Entities;
using Microsoft.Extensions.Logging;

public class InMemoryApplicationRepository : IApplicationRepository
{
    private readonly List<Application> applications;
    private readonly List<ApplicationRole> applicationRoles;
    private readonly List<UserApplicationRole> userApplicationRoles;
    private readonly List<ApplicationScope> applicationScopes;
    private readonly List<Scope> scopes;
    private readonly ILogger<InMemoryApplicationRepository> logger;

    public InMemoryApplicationRepository(InMemoryDataContext dataContext, ILogger<InMemoryApplicationRepository> logger)
    {
        if (dataContext is null)
        {
            throw new ArgumentNullException(nameof(dataContext));
        }

        applications = dataContext.Applications;
        applicationRoles = dataContext.ApplicationRoles;
        userApplicationRoles = dataContext.UserApplicationRoles;
        applicationScopes = dataContext.ApplicationScopes;
        scopes = dataContext.Scopes;
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<IEnumerable<Application>> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(applications.AsEnumerable());
        }
        catch (Exception exception)
        {
            logger.ApplicationsLoadingError(exception);
            throw;
        }
    }
    public Task<Application> GetByCode(string code, CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(applications.FirstOrDefault(u => u.Code.Equals(code, StringComparison.OrdinalIgnoreCase)));
        }
        catch (Exception exception)
        {
            logger.ApplicationsLoadingError(exception);
            throw;
        }
    }

    public Task<Application> GetById(Domain.Entities.ApplicationId applicationId, CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(applications.FirstOrDefault(u => u.Id == applicationId));
        }
        catch (Exception exception)
        {
            logger.ApplicationsLoadingError(exception);
            throw;
        }
    }

    public Task<IEnumerable<ApplicationRole>> GetRoles(Domain.Entities.ApplicationId applicationId, CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(applicationRoles.Where(r => r.ApplicationId == applicationId));
        }
        catch (Exception exception)
        {
            logger.ApplicationRolesLoadingError(exception);
            throw;
        }
    }

    public Task<IEnumerable<ApplicationRole>> GetUserRoles(Domain.Entities.ApplicationId applicationId, UserId userId, CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(applicationRoles.Where(r => r.ApplicationId == applicationId)
                .Join(userApplicationRoles
                .Where(uar => uar.UserId == userId), ar => ar.Id, uar => uar.ApplicationRoleId, (ar, uar) => ar));
        }
        catch (Exception exception)
        {
            logger.ApplicationRolesLoadingError(exception);
            throw;
        }
    }

    public Task<Application> Create(Application application, CancellationToken cancellationToken)
    {
        try
        {
            application.Id = Domain.Entities.ApplicationId.New();
            applications.Add(application);
            return Task.FromResult(application);
        }
        catch (Exception exception)
        {
            logger.ApplicationsCreatingError(exception);
            throw;
        }
    }

    public Task<Application> Update(Application application, CancellationToken cancellationToken)
    {
        try
        {
            var storedApplication = applications.FirstOrDefault(a => a.Id == application.Id);

            if (storedApplication == null)
            {
                throw new NotFoundException(nameof(Application), application.Id);
            }

            applications.Remove(storedApplication);
            applications.Add(application);

            return Task.FromResult(application);
        }
        catch (Exception exception)
        {
            logger.ApplicationsUpdatingError(exception);
            throw;
        }
    }

    public Task Delete(Domain.Entities.ApplicationId applicationId, CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(applications.RemoveAll(a => a.Id == applicationId));
        }
        catch (Exception exception)
        {
            logger.ApplicationsDeletingError(exception);
            throw;
        }
    }

    public Task Clear(CancellationToken cancellationToken)
    {
        applications.Clear();
        return Task.CompletedTask;
    }

    public Task DeleteApplicationScopes(string name, CancellationToken cancellationToken)
    {
        try
        {
            var scope = scopes.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (scope is null)
            {
                return Task.CompletedTask;
            }

            return Task.FromResult(applicationScopes.RemoveAll(s => s.ScopeId == scope.Id));
        }
        catch (Exception exception)
        {
            logger.ScopesDeletingError(exception);
            throw;
        }
    }

    public Task<IEnumerable<string>> GetApplicationScopes(Domain.Entities.ApplicationId applicationId, CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(applicationScopes.Where(r => r.ApplicationId == applicationId)
                .Join(scopes, ascope => ascope.ScopeId, s => s.Id, (asc, s) => s.Name));
        }
        catch (Exception exception)
        {
            logger.ScopesLoadingError(exception);
            throw;
        }
    }
}
