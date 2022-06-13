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

public class InMemoryApplicationRoleRepository : IApplicationRoleRepository
{
    private const string ERROR_LOADING_APPLICATION_ROLES = "Error loading application roles";
    private const string ERROR_INSERTING_APPLICATION_ROLES = "Error inserting application roles";
    private const string ERROR_UPDATING_APPLICATION_ROLES = "Error updating application roles";
    private const string ERROR_DELETING_APPLICATION_ROLES = "Error deleting application roles";
    private readonly InMemoryDataContext dataContext;
    private readonly ILogger<InMemoryApplicationRoleRepository> logger;

    public InMemoryApplicationRoleRepository(InMemoryDataContext dataContext, ILogger<InMemoryApplicationRoleRepository> logger)
    {
        this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<IEnumerable<ApplicationRole>> GetByApplicationId(Domain.Entities.ApplicationId applicationId, CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(dataContext.ApplicationRoles.Where(u => u.ApplicationId == applicationId));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_LOADING_APPLICATION_ROLES);
            throw;
        }
    }

    public Task<ApplicationRole> GetById(ApplicationRoleId applicationRoleId, CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(dataContext.ApplicationRoles.FirstOrDefault(u => u.Id == applicationRoleId));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_LOADING_APPLICATION_ROLES);
            throw;
        }
    }

    public Task<ApplicationRole> Create(ApplicationRole role, CancellationToken cancellationToken)
    {
        try
        {
            role.Id = ApplicationRoleId.New();
            dataContext.ApplicationRoles.Add(role);
            return Task.FromResult(role);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_INSERTING_APPLICATION_ROLES);
            throw;
        }
    }

    public Task<ApplicationRole> Update(ApplicationRole role, CancellationToken cancellationToken)
    {
        try
        {
            var storedRole = dataContext.ApplicationRoles.FirstOrDefault(a => a.Id == role.Id);

            if (storedRole == null)
            {
                throw new NotFoundException(nameof(Application), role.Id);
            }

            dataContext.ApplicationRoles.Remove(storedRole);
            dataContext.ApplicationRoles.Add(role);

            return Task.FromResult(role);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_UPDATING_APPLICATION_ROLES);
            throw;
        }
    }

    public Task Delete(ApplicationRoleId roleId, CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(dataContext.ApplicationRoles.RemoveAll(a => a.Id == roleId));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_DELETING_APPLICATION_ROLES);
            throw;
        }
    }


    //public Task<Application> Update(Application application, CancellationToken cancellationToken)
    //{
    //    try
    //    {
    //        var storedApplication = dataContext.Applications.FirstOrDefault(a => a.Id == application.Id);

    //        if (storedApplication == null)
    //        {
    //            throw new NotFoundException(nameof(Application), application.Id);
    //        }

    //        dataContext.Applications.Remove(storedApplication);
    //        dataContext.Applications.Add(application);

    //        return Task.FromResult(application);
    //    }
    //    catch (Exception exception)
    //    {
    //        logger.LogError(exception, ERROR_UPDATING_APPLICATION_ROLES);
    //        throw;
    //    }
    //}
}
