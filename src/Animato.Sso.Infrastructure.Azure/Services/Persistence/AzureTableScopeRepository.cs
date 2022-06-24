namespace Animato.Sso.Infrastructure.AzureStorage.Services.Persistence;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Common.Logging;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Domain.Entities;
using Animato.Sso.Infrastructure.AzureStorage.Services.Persistence.DTOs;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;

public class AzureTableScopeRepository : IScopeRepository
{
    private TableClient TableScopes => dataContext.Scopes;
    private TableClient TableApplicationScopes => dataContext.ApplicationScopes;

    private Func<CancellationToken, Task> CheckIfTableExists => dataContext.ThrowExceptionIfTableNotExists;
    private readonly AzureTableStorageDataContext dataContext;
    private readonly ILogger<AzureTableScopeRepository> logger;

    public AzureTableScopeRepository(AzureTableStorageDataContext dataContext, ILogger<AzureTableScopeRepository> logger)
    {
        this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private Task ThrowExceptionIfTableNotExists(CancellationToken cancellationToken)
        => CheckIfTableExists(cancellationToken);

    public Task<Scope> Create(Scope scope, CancellationToken cancellationToken)
        => Create(scope, ScopeId.New(), cancellationToken);

    public async Task<Scope> Create(Scope scope, ScopeId id, CancellationToken cancellationToken)
    {
        if (scope is null)
        {
            throw new ArgumentNullException(nameof(scope));
        }

        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            scope.Id = id;
            var tableEntity = scope.ToTableEntity();
            await TableScopes.AddEntityAsync(tableEntity, cancellationToken);
            return scope;
        }
        catch (Exception exception)
        {
            logger.ApplicationsCreatingError(exception);
            throw;
        }
    }

    public async Task<Scope> Update(string oldName, string newName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(oldName))
        {
            throw new ArgumentNullException(nameof(oldName));
        }

        if (string.IsNullOrEmpty(newName))
        {
            throw new ArgumentNullException(nameof(newName));
        }

        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var scope = await GetScopeByName(oldName, cancellationToken);

            if (scope == null)
            {
                throw new NotFoundException(nameof(Scope), oldName);
            }

            scope.Name = newName;
            await TableScopes.UpdateEntityAsync(scope.ToTableEntity(), Azure.ETag.All, cancellationToken: cancellationToken);
            return scope;
        }
        catch (Exception exception)
        {
            logger.ScopesUpdatingError(exception);
            throw;
        }
    }

    public async Task Delete(string name, CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        var scope = await GetScopeByName(name, cancellationToken);

        if (scope == null)
        {
            return;
        }

        var tableEntity = scope.ToTableEntity();

        try
        {
            await TableScopes.DeleteEntityAsync(tableEntity.PartitionKey, tableEntity.RowKey, cancellationToken: cancellationToken);
        }
        catch (Exception exception)
        {
            logger.ScopesDeletingError(exception);
            throw;
        }
    }

    public Task<IEnumerable<Scope>> Create(CancellationToken cancellationToken, params Scope[] scopes) => throw new NotImplementedException();

    public async Task<IEnumerable<Scope>> GetAll(CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var results = new List<ScopeTableEntity>();
            var queryResult = TableScopes.QueryAsync<ScopeTableEntity>(cancellationToken: cancellationToken, maxPerPage: AzureTableStorageDataContext.MAX_PER_PAGE);
            await queryResult.AsPages()
                .ForEachAsync(page => results.AddRange(page.Values), cancellationToken)
                .ConfigureAwait(false);

            return results.Select(e => e.ToEntity());
        }
        catch (Exception exception)
        {
            logger.ScopesLoadingError(exception);
            throw;
        }
    }

    public async Task<Scope> GetScopeByName(string name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
        }

        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var results = new List<ScopeTableEntity>();
            var queryResult = TableScopes.QueryAsync<ScopeTableEntity>(s => s.PartitionKey == name, cancellationToken: cancellationToken, maxPerPage: AzureTableStorageDataContext.MAX_PER_PAGE);
            await queryResult.AsPages()
                .ForEachAsync(page => results.AddRange(page.Values), cancellationToken)
                .ConfigureAwait(false);

            return results.FirstOrDefault()?.ToEntity();
        }
        catch (Exception exception)
        {
            logger.ScopesLoadingError(exception);
            throw;
        }
    }

    public async Task<IEnumerable<Scope>> GetScopesByName(CancellationToken cancellationToken, params string[] names)
    {
        if (names is null || !names.Any())
        {
            return Enumerable.Empty<Scope>();
        }

        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var results = new List<ScopeTableEntity>();
            var filter = string.Join(" or ", names.Select(n => $"PartitionKey eq '{n}'"));
            var queryResult = TableScopes.QueryAsync<ScopeTableEntity>(filter, cancellationToken: cancellationToken, maxPerPage: AzureTableStorageDataContext.MAX_PER_PAGE);
            await queryResult.AsPages()
                .ForEachAsync(page => results.AddRange(page.Values), cancellationToken)
                .ConfigureAwait(false);

            return results.Select(s => s.ToEntity());
        }
        catch (Exception exception)
        {
            logger.ScopesLoadingError(exception);
            throw;
        }
    }

    public async Task<IEnumerable<Scope>> GetScopesById(CancellationToken cancellationToken, params ScopeId[] ids)
    {
        if (ids is null || !ids.Any())
        {
            return Enumerable.Empty<Scope>();
        }

        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var results = new List<ScopeTableEntity>();
            var filter = string.Join(" or ", ids.Select(n => $"RowKey eq '{n.Value}'"));
            var queryResult = TableScopes.QueryAsync<ScopeTableEntity>(filter, cancellationToken: cancellationToken, maxPerPage: AzureTableStorageDataContext.MAX_PER_PAGE);
            await queryResult.AsPages()
                .ForEachAsync(page => results.AddRange(page.Values), cancellationToken)
                .ConfigureAwait(false);

            return results.Select(s => s.ToEntity());
        }
        catch (Exception exception)
        {
            logger.ScopesLoadingError(exception);
            throw;
        }
    }
}
