namespace Animato.Sso.Infrastructure.AzureStorage.Services.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Common.Logging;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Domain.Entities;
using Animato.Sso.Infrastructure.AzureStorage.Services.Persistence.DTOs;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;

public class AzureTableApplicationRoleRepository : IApplicationRoleRepository
{
    private TableClient Table => dataContext.ApplicationRoles;
    private Func<CancellationToken, Task> CheckIfTableExists => dataContext.ThrowExceptionIfTableNotExists;
    private readonly AzureTableStorageDataContext dataContext;
    private readonly ILogger<AzureTableApplicationRoleRepository> logger;

    public AzureTableApplicationRoleRepository(AzureTableStorageDataContext dataContext, ILogger<AzureTableApplicationRoleRepository> logger)
    {
        this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private Task ThrowExceptionIfTableNotExists(CancellationToken cancellationToken)
        => CheckIfTableExists(cancellationToken);

    public async Task<IEnumerable<ApplicationRole>> GetByApplicationId(Domain.Entities.ApplicationId applicationId, CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var queryResult = Table.QueryAsync<ApplicationRoleTableEntity>(a => a.PartitionKey == applicationId.ToString(), cancellationToken: cancellationToken);
            var results = new List<ApplicationRoleTableEntity>();

            await queryResult.AsPages()
                .ForEachAsync(page => results.AddRange(page.Values), cancellationToken)
                .ConfigureAwait(false);

            return results.Select(r => r.ToEntity());
        }
        catch (Exception exception)
        {
            logger.ApplicationRolesLoadingError(exception);
            throw;
        }
    }

    public async Task<ApplicationRole> GetById(ApplicationRoleId applicationRoleId, CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var queryResult = Table.QueryAsync<ApplicationRoleTableEntity>(a => a.RowKey == applicationRoleId.ToString(), cancellationToken: cancellationToken);
            var results = new List<ApplicationRoleTableEntity>();

            await queryResult.AsPages()
                .ForEachAsync(page => results.AddRange(page.Values), cancellationToken)
                .ConfigureAwait(false);

            if (results.Count == 1)
            {
                return results.First().ToEntity();
            }

            if (results.Count == 0)
            {
                return null;
            }

            throw new DataAccessException($"Found duplicate application roles ({results.Count}) for id {applicationRoleId.Value}");

        }
        catch (Exception exception)
        {
            logger.ApplicationRolesLoadingError(exception);
            throw;
        }
    }

    public async Task<ApplicationRole> Create(ApplicationRole role, CancellationToken cancellationToken)
    {
        if (role is null)
        {
            throw new ArgumentNullException(nameof(role));
        }

        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            role.Id = ApplicationRoleId.New();
            var tableEntity = role.ToTableEntity();
            await Table.AddEntityAsync(tableEntity, cancellationToken);
            return role;
        }
        catch (Exception exception)
        {
            logger.ApplicationRolesInsertingError(exception);
            throw;
        }
    }

    public async Task<ApplicationRole> Update(ApplicationRole role, CancellationToken cancellationToken)
    {
        if (role is null)
        {
            throw new ArgumentNullException(nameof(role));
        }

        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var tableEntity = role.ToTableEntity();
            await Table.UpdateEntityAsync(tableEntity, Azure.ETag.All, cancellationToken: cancellationToken);
            return role;

        }
        catch (Exception exception)
        {
            logger.ApplicationRolesUpdatingError(exception);
            throw;
        }
    }

    public async Task Delete(ApplicationRoleId roleId, CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var role = await GetById(roleId, cancellationToken);

            if (role == null)
            {
                return;
            }

            var tableEntity = role.ToTableEntity();
            await Table.DeleteEntityAsync(tableEntity.PartitionKey, tableEntity.RowKey, cancellationToken: cancellationToken);
        }
        catch (Exception exception)
        {
            logger.ApplicationRolesDeletingError(exception);
            throw;
        }
    }

    public async Task Clear(CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            await AzureTableStorageDataContext.DeleteAllEntitiesAsync(Table, CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.ApplicationRolesDeletingError(exception);
            throw;
        }
    }
}
