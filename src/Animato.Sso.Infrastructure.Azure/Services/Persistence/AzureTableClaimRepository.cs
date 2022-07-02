namespace Animato.Sso.Infrastructure.AzureStorage.Services.Persistence;

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

public class AzureTableClaimRepository : IClaimRepository
{
    private Func<CancellationToken, Task> CheckIfTableExists => dataContext.ThrowExceptionIfTableNotExists;
    private readonly AzureTableStorageDataContext dataContext;
    private readonly IScopeRepository scopeRepository;
    private readonly ILogger<AzureTableClaimRepository> logger;
    private TableClient TableClaims => dataContext.Claims;
    private TableClient TableUserClaims => dataContext.UserClaims;
    private TableClient TableClaimScopes => dataContext.ClaimScopes;

    public AzureTableClaimRepository(AzureTableStorageDataContext dataContext
        , IScopeRepository scopeRepository
        , ILogger<AzureTableClaimRepository> logger)
    {
        this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        this.scopeRepository = scopeRepository ?? throw new ArgumentNullException(nameof(scopeRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private Task ThrowExceptionIfTableNotExists(CancellationToken cancellationToken)
        => CheckIfTableExists(cancellationToken);

    public Task<Claim> Create(Claim claim, CancellationToken cancellationToken)
        => Create(claim, ClaimId.New(), cancellationToken);

    public async Task<Claim> Create(Claim claim, ClaimId id, CancellationToken cancellationToken)
    {
        if (claim is null)
        {
            throw new ArgumentNullException(nameof(claim));
        }

        await ThrowExceptionIfTableNotExists(cancellationToken);

        claim.Id = id;

        try
        {
            var tableEntity = claim.ToTableEntity();
            await TableClaims.AddEntityAsync(tableEntity, cancellationToken);
            return claim;
        }
        catch (Exception exception)
        {
            logger.ScopesCreatingError(exception);
            throw;
        }
    }

    public async Task<Claim> Update(Claim claim, CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var storedClaim = await GetBydId(claim.Id, cancellationToken);

            if (storedClaim == null)
            {
                throw new NotFoundException(nameof(Claim), claim.Id);
            }

            var tableEntity = claim.ToTableEntity();
            await TableClaims.UpdateEntityAsync(tableEntity, Azure.ETag.All, cancellationToken: cancellationToken);
            return claim;
        }
        catch (Exception exception)
        {
            logger.ClaimsUpdatingError(exception);
            throw;
        }
    }

    public async Task Delete(string name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
        }

        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var claim = await GetClaimByName(name, cancellationToken);

            if (claim is null)
            {
                return;
            }

            await DeleteScopesByClaim(claim.Id, cancellationToken);
            await DeleteUserClaims(claim.Id, cancellationToken);
            var tableEntity = claim.ToTableEntity();
            await TableClaims.DeleteEntityAsync(tableEntity.PartitionKey, tableEntity.RowKey, cancellationToken: cancellationToken);

        }
        catch (Exception exception)
        {
            logger.ClaimsDeletingError(exception);
            throw;
        }
    }

    public async Task DeleteUserClaims(ClaimId id, CancellationToken cancellationToken)
    {
        try
        {
            var results = new List<UserClaimTableEntity>();
            var queryResult = TableUserClaims.QueryAsync<UserClaimTableEntity>(
                s => s.ClaimId == id.Value.ToString()
                , cancellationToken: cancellationToken);

            await queryResult.AsPages()
                .ForEachAsync(page => results.AddRange(page.Values), cancellationToken)
                .ConfigureAwait(false);

            await AzureTableStorageDataContext.BatchManipulateEntities(TableUserClaims
                , results
                , TableTransactionActionType.Delete
                , cancellationToken
                );
        }
        catch (Exception exception)
        {
            logger.ClaimsDeletingError(exception);
            throw;
        }
    }

    public async Task DeleteScopesByClaim(ClaimId id, CancellationToken cancellationToken)
    {
        try
        {
            var results = new List<ClaimScopeTableEntity>();
            var queryResult = TableUserClaims.QueryAsync<ClaimScopeTableEntity>(
                s => s.RowKey == id.Value.ToString()
                , cancellationToken: cancellationToken);

            await queryResult.AsPages()
                .ForEachAsync(page => results.AddRange(page.Values), cancellationToken)
                .ConfigureAwait(false);

            await AzureTableStorageDataContext.BatchManipulateEntities(TableUserClaims
                , results
                , TableTransactionActionType.Delete
                , cancellationToken
                );
        }
        catch (Exception exception)
        {
            logger.ClaimsDeletingError(exception);
            throw;
        }
    }

    public async Task DeleteScopesByScope(ScopeId scopeId, CancellationToken cancellationToken)
    {
        try
        {
            var results = new List<ClaimScopeTableEntity>();
            var queryResult = TableUserClaims.QueryAsync<ClaimScopeTableEntity>(
                s => s.PartitionKey == scopeId.Value.ToString()
                , cancellationToken: cancellationToken);

            await queryResult.AsPages()
                .ForEachAsync(page => results.AddRange(page.Values), cancellationToken)
                .ConfigureAwait(false);

            await AzureTableStorageDataContext.BatchManipulateEntities(TableUserClaims
                , results
                , TableTransactionActionType.Delete
                , cancellationToken
                );
        }
        catch (Exception exception)
        {
            logger.ClaimsDeletingError(exception);
            throw;
        }
    }

    public async Task<IEnumerable<Claim>> GetAll(CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var results = new List<ClaimTableEntity>();
            var queryResult = TableClaims.QueryAsync<ClaimTableEntity>(cancellationToken: cancellationToken);

            await queryResult.AsPages()
                .ForEachAsync(page => results.AddRange(page.Values), cancellationToken)
                .ConfigureAwait(false);

            return results.Select(u => u.ToEntity());
        }
        catch (Exception exception)
        {
            logger.ClaimsLoadingError(exception);
            throw;
        }
    }

    public async Task<Claim> GetBydId(ClaimId id, CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var results = new List<ClaimTableEntity>();
            var queryResult = TableClaims.QueryAsync<ClaimTableEntity>(c => c.RowKey == id.ToString(), cancellationToken: cancellationToken);

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

            throw new DataAccessException($"Found duplicate claims ({results.Count}) for id {id.Value}");
        }
        catch (Exception exception)
        {
            logger.ClaimsLoadingError(exception);
            throw;
        }
    }

    public async Task<IEnumerable<Claim>> GetBydId(CancellationToken cancellationToken, params ClaimId[] ids)
    {
        if (ids is null || !ids.Any())
        {
            return Enumerable.Empty<Claim>();
        }

        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var results = new List<ClaimTableEntity>();
            var filter = string.Join(" or ", ids.Select(id => $"RowKey eq '{id.Value}'"));
            var queryResult = TableClaims.QueryAsync<ClaimTableEntity>(filter, cancellationToken: cancellationToken);

            await queryResult.AsPages()
                .ForEachAsync(page => results.AddRange(page.Values), cancellationToken)
                .ConfigureAwait(false);

            return results.Select(s => s.ToEntity());
        }
        catch (Exception exception)
        {
            logger.ClaimsLoadingError(exception);
            throw;
        }
    }

    public async Task<Claim> GetClaimByName(string name, CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var results = new List<ClaimTableEntity>();
            var queryResult = TableClaims.QueryAsync<ClaimTableEntity>(c => c.PartitionKey == name, cancellationToken: cancellationToken);

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

            throw new DataAccessException($"Found duplicate claims ({results.Count}) for name {name}");
        }
        catch (Exception exception)
        {
            logger.ClaimsLoadingError(exception);
            throw;
        }
    }

    public async Task<IEnumerable<Claim>> GetByScope(string scopeName, int topCount, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(scopeName))
        {
            throw new ArgumentException($"'{nameof(scopeName)}' cannot be null or empty.", nameof(scopeName));
        }

        if (topCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(topCount), "Must be greater than 0");
        }

        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {

            var scope = await scopeRepository.GetScopeByName(scopeName, cancellationToken);

            if (scope is null)
            {
                return Enumerable.Empty<Claim>();
            }

            var claimScopes = await GetClaimScopesByScope(scope.Id, cancellationToken);

            if (!claimScopes.Any())
            {
                return Enumerable.Empty<Claim>();
            }

            var claimIds = claimScopes.Select(c => c.ClaimId).Distinct().Take(topCount);
            return await GetBydId(cancellationToken, claimIds.ToArray());
        }
        catch (Exception exception)
        {
            logger.ClaimsLoadingError(exception);
            throw;
        }
    }

    private async Task<IEnumerable<ClaimScope>> GetClaimScopesByScope(ScopeId id, CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var results = new List<ClaimScopeTableEntity>();
            var queryResult = TableClaims.QueryAsync<ClaimScopeTableEntity>(c => c.PartitionKey == id.ToString(), cancellationToken: cancellationToken);

            await queryResult.AsPages()
                .ForEachAsync(page => results.AddRange(page.Values), cancellationToken)
                .ConfigureAwait(false);

            return results.Select(c => c.ToEntity());
        }
        catch (Exception exception)
        {
            logger.ClaimsLoadingError(exception);
            throw;
        }
    }

    public async Task Clear(CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            await AzureTableStorageDataContext.DeleteAllEntitiesAsync(TableUserClaims, CancellationToken.None);
            await AzureTableStorageDataContext.DeleteAllEntitiesAsync(TableClaimScopes, CancellationToken.None);
            await AzureTableStorageDataContext.DeleteAllEntitiesAsync(TableClaims, CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.ApplicationRolesDeletingError(exception);
            throw;
        }
    }

    public Task<IEnumerable<Claim>> GetByName(CancellationToken cancellationToken, params string[] names) => throw new NotImplementedException();
    public Task<IEnumerable<Claim>> GetByScope(string scopeName, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<IEnumerable<Claim>> GetByScope(ScopeId scopeId, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<ClaimScope> GetClaimScope(ScopeId scopeId, ClaimId claimId, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<ClaimScope> AddClaimScope(ClaimScope claimScope, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task RemoveClaimScope(ScopeId scopeId, ClaimId claimId, CancellationToken cancellationToken) => throw new NotImplementedException();
}
