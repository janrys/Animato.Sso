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
    private TableClient TableUserApplicationRoles => dataContext.UserApplicationRoles;
    private TableClient TableApplicationRoles => dataContext.ApplicationRoles;
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

    public async Task<IEnumerable<ApplicationRole>> GetByApplication(Domain.Entities.ApplicationId applicationId, CancellationToken cancellationToken)
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

    public async Task<IEnumerable<ApplicationRole>> GetByIds(CancellationToken cancellationToken, params ApplicationRoleId[] applicationRoleIds)
    {
        var applicationRoles = new List<ApplicationRole>();

        if (applicationRoleIds is null || !applicationRoleIds.Any())
        {
            return applicationRoles;
        }

        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var filter = string.Join(" or ", applicationRoleIds.Select(r => $"RowKey eq '{r.Value}'"));
            var queryResult = Table.QueryAsync<ApplicationRoleTableEntity>(filter, cancellationToken: cancellationToken);
            var results = new List<ApplicationRoleTableEntity>();

            await queryResult.AsPages()
                .ForEachAsync(page => results.AddRange(page.Values), cancellationToken)
                .ConfigureAwait(false);

            applicationRoles.AddRange(results.Select(e => e.ToEntity()));
            return applicationRoles;
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
            logger.ApplicationRolesCreatingError(exception);
            throw;
        }
    }

    public async Task<IEnumerable<ApplicationRole>> Create(CancellationToken cancellationToken, params ApplicationRole[] roles)
    {
        if (roles is null || !roles.Any())
        {
            throw new ArgumentNullException(nameof(roles));
        }

        await ThrowExceptionIfTableNotExists(cancellationToken);

        roles.ToList().ForEach(r => r.Id = ApplicationRoleId.New());
        await AzureTableStorageDataContext.BatchManipulateEntities(
            Table
            , roles.Select(r => r.ToTableEntity())
            , TableTransactionActionType.Add
            , cancellationToken);

        return roles;
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

    public async Task<IEnumerable<ApplicationRole>> GetRoles(Domain.Entities.ApplicationId applicationId, CancellationToken cancellationToken)
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

    public async Task<IEnumerable<ApplicationRole>> GetByUser(UserId id, CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var userApplicationRoles = new List<UserApplicationRoleTableEntity>();
            var queryResult = TableUserApplicationRoles
                .QueryAsync<UserApplicationRoleTableEntity>(r => r.RowKey == id.Value.ToString()
                , cancellationToken: cancellationToken);

            await queryResult.AsPages()
                .ForEachAsync(page => userApplicationRoles.AddRange(page.Values), cancellationToken)
                .ConfigureAwait(false);

            if (userApplicationRoles.Count == 0)
            {
                return Enumerable.Empty<ApplicationRole>();
            }

            var applicationRoles = new List<ApplicationRoleTableEntity>();

            foreach (var userRole in userApplicationRoles)
            {
                var queryResultApplicationRoles = TableApplicationRoles
                .QueryAsync<ApplicationRoleTableEntity>(r => r.RowKey == userRole.ApplicationRoleId
                , cancellationToken: cancellationToken);

                await queryResultApplicationRoles.AsPages()
                    .ForEachAsync(page => applicationRoles.AddRange(page.Values), cancellationToken)
                    .ConfigureAwait(false);
            }

            return applicationRoles.Select(r => r.ToEntity());
        }
        catch (Exception exception)
        {
            logger.ApplicationRolesLoadingError(exception);
            throw;
        }
    }


    public async Task<IEnumerable<ApplicationRole>> GetByApplicationAndUser(Domain.Entities.ApplicationId applicationId, UserId userId, CancellationToken cancellationToken)
    {
        var applicationRoles = await GetRoles(applicationId, cancellationToken);
        var userRoles = await GetUserApplicationRoles(userId, cancellationToken);

        return applicationRoles
            .Join(userRoles
            .Where(uar => uar.UserId == userId), ar => ar.Id, uar => uar.ApplicationRoleId, (ar, uar) => ar);
    }

    private async Task<IEnumerable<UserApplicationRole>> GetUserApplicationRoles(UserId userId, CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var queryResult = TableUserApplicationRoles.QueryAsync<UserApplicationRoleTableEntity>(a => a.RowKey == userId.ToString(), cancellationToken: cancellationToken);
            var results = new List<UserApplicationRoleTableEntity>();

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
