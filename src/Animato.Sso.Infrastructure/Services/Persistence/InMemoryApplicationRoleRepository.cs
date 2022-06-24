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

public class InMemoryApplicationRoleRepository : IApplicationRoleRepository
{
    private readonly List<ApplicationRole> applicationRoles;
    private readonly ILogger<InMemoryApplicationRoleRepository> logger;

    public InMemoryApplicationRoleRepository(InMemoryDataContext dataContext, ILogger<InMemoryApplicationRoleRepository> logger)
    {
        if (dataContext is null)
        {
            throw new ArgumentNullException(nameof(dataContext));
        }

        applicationRoles = dataContext.ApplicationRoles;
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<IEnumerable<ApplicationRole>> GetByApplicationId(Domain.Entities.ApplicationId applicationId, CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(applicationRoles.Where(u => u.ApplicationId == applicationId));
        }
        catch (Exception exception)
        {
            logger.ApplicationRolesLoadingError(exception);
            throw;
        }
    }

    public Task<ApplicationRole> GetById(ApplicationRoleId applicationRoleId, CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(applicationRoles.FirstOrDefault(u => u.Id == applicationRoleId));
        }
        catch (Exception exception)
        {
            logger.ApplicationRolesLoadingError(exception);
            throw;
        }
    }

    public Task<IEnumerable<ApplicationRole>> GetByIds(CancellationToken cancellationToken, params ApplicationRoleId[] applicationRoleIds)
    {
        if (applicationRoleIds is null || !applicationRoleIds.Any())
        {
            return Task.FromResult(Enumerable.Empty<ApplicationRole>());
        }

        try
        {
            return Task.FromResult(applicationRoles.Where(r => applicationRoleIds.Any(a => a == r.Id)));
        }
        catch (Exception exception)
        {
            logger.ApplicationRolesLoadingError(exception);
            throw;
        }
    }

    public Task<ApplicationRole> Create(ApplicationRole role, CancellationToken cancellationToken)
    {
        try
        {
            role.Id = ApplicationRoleId.New();
            applicationRoles.Add(role);
            return Task.FromResult(role);
        }
        catch (Exception exception)
        {
            logger.ApplicationRolesCreatingError(exception);
            throw;
        }
    }

    public async Task<IEnumerable<ApplicationRole>> Create(CancellationToken cancellationToken, params ApplicationRole[] roles)
    {
        var result = new List<ApplicationRole>();

        foreach (var role in roles)
        {
            result.Add(await Create(role, cancellationToken));
        }

        return result;
    }


    public Task<ApplicationRole> Update(ApplicationRole role, CancellationToken cancellationToken)
    {
        try
        {
            var storedRole = applicationRoles.FirstOrDefault(a => a.Id == role.Id);

            if (storedRole == null)
            {
                throw new NotFoundException(nameof(Application), role.Id);
            }

            applicationRoles.Remove(storedRole);
            applicationRoles.Add(role);

            return Task.FromResult(role);
        }
        catch (Exception exception)
        {
            logger.ApplicationRolesUpdatingError(exception);
            throw;
        }
    }

    public Task Delete(ApplicationRoleId roleId, CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(applicationRoles.RemoveAll(a => a.Id == roleId));
        }
        catch (Exception exception)
        {
            logger.ApplicationRolesDeletingError(exception);
            throw;
        }
    }

    public Task Clear(CancellationToken cancellationToken)
    {
        applicationRoles.Clear();
        return Task.CompletedTask;
    }

}
