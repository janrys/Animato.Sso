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

public class AzureTableApplicationRepository : IApplicationRepository
{
    private TableClient TableApplications => dataContext.Applications;
    private TableClient TableApplicationRoles => dataContext.ApplicationRoles;
    private TableClient TableUserApplicationRoles => dataContext.UserApplicationRoles;
    private Func<CancellationToken, Task> CheckIfTableExists => dataContext.ThrowExceptionIfTableNotExists;
    private readonly AzureTableStorageDataContext dataContext;
    private readonly ILogger<AzureTableApplicationRepository> logger;

    public AzureTableApplicationRepository(AzureTableStorageDataContext dataContext, ILogger<AzureTableApplicationRepository> logger)
    {
        this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private Task ThrowExceptionIfTableNotExists(CancellationToken cancellationToken)
        => CheckIfTableExists(cancellationToken);

    public async Task<IEnumerable<Application>> GetAll(CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var queryResult = TableApplications.QueryAsync<ApplicationTableEntity>(cancellationToken: cancellationToken, maxPerPage: AzureTableStorageDataContext.MAX_PER_PAGE);
            var results = new List<ApplicationTableEntity>();

            await queryResult.AsPages()
                .ForEachAsync(page => results.AddRange(page.Values), cancellationToken)
                .ConfigureAwait(false);

            return results.Select(e => e.ToEntity());
        }
        catch (Exception exception)
        {
            logger.ApplicationsLoadingError(exception);
            throw;
        }
    }
    public async Task<Application> GetByCode(string code, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(code))
        {
            throw new ArgumentException($"'{nameof(code)}' cannot be null or empty.", nameof(code));
        }

        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var queryResult = TableApplications.QueryAsync<ApplicationTableEntity>(a => a.PartitionKey == code, cancellationToken: cancellationToken);
            var results = new List<ApplicationTableEntity>();

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

            throw new DataAccessException($"Found duplicate applications ({results.Count}) for code {code}");
        }
        catch (Exception exception)
        {
            logger.ApplicationsLoadingError(exception);
            throw;
        }
    }

    public async Task<Application> GetById(Domain.Entities.ApplicationId applicationId, CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var results = new List<ApplicationTableEntity>();
            var queryResult = TableApplications.QueryAsync<ApplicationTableEntity>(a => a.RowKey == applicationId.Value.ToString(), cancellationToken: cancellationToken);

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

            throw new DataAccessException($"Found duplicate applications ({results.Count}) for id {applicationId.Value}");
        }
        catch (Exception exception)
        {
            logger.ApplicationsLoadingError(exception);
            throw;
        }
    }

    public async Task<IEnumerable<ApplicationRole>> GetRoles(Domain.Entities.ApplicationId applicationId, CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var queryResult = TableApplicationRoles.QueryAsync<ApplicationRoleTableEntity>(a => a.PartitionKey == applicationId.ToString(), cancellationToken: cancellationToken);
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

    public async Task<IEnumerable<ApplicationRole>> GetUserRoles(Domain.Entities.ApplicationId applicationId, UserId userId, CancellationToken cancellationToken)
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

    public async Task<Application> Create(Application application, CancellationToken cancellationToken)
    {
        if (application is null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            application.Id = Domain.Entities.ApplicationId.New();
            var tableEntity = application.ToTableEntity();
            await TableApplications.AddEntityAsync(tableEntity, cancellationToken);
            return application;
        }
        catch (Exception exception)
        {
            logger.ApplicationsInsertingError(exception);
            throw;
        }
    }

    public async Task<Application> Update(Application application, CancellationToken cancellationToken)
    {
        if (application is null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var tableEntity = application.ToTableEntity();
            await TableApplications.UpdateEntityAsync(tableEntity, Azure.ETag.All, cancellationToken: cancellationToken);
            return application;
        }
        catch (Exception exception)
        {
            logger.ApplicationsUpdatingError(exception);
            throw;
        }
    }

    public async Task Delete(Domain.Entities.ApplicationId applicationId, CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        var application = await GetById(applicationId, cancellationToken);

        if (application == null)
        {
            return;
        }

        var tableEntity = application.ToTableEntity();

        try
        {
            await TableApplications.DeleteEntityAsync(tableEntity.PartitionKey, tableEntity.RowKey, cancellationToken: cancellationToken);
        }
        catch (Exception exception)
        {
            logger.ApplicationsDeletingError(exception);
            throw;
        }
    }
    public async Task Clear(CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            await AzureTableStorageDataContext.DeleteAllEntitiesAsync(TableApplications, CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.ApplicationRolesDeletingError(exception);
            throw;
        }
    }
}
