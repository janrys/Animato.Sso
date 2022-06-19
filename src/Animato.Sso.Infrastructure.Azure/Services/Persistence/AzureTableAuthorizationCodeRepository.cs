namespace Animato.Sso.Infrastructure.AzureStorage.Services.Persistence;
using System;
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

public class AzureTableAuthorizationCodeRepository : IAuthorizationCodeRepository
{
    private TableClient Table => dataContext.AuthorizationCodes;
    private Func<CancellationToken, Task> CheckIfTableExists => dataContext.ThrowExceptionIfTableNotExists;
    private readonly AzureTableStorageDataContext dataContext;
    private readonly ILogger<AzureTableAuthorizationCodeRepository> logger;

    public AzureTableAuthorizationCodeRepository(AzureTableStorageDataContext dataContext, ILogger<AzureTableAuthorizationCodeRepository> logger)
    {
        this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private Task ThrowExceptionIfTableNotExists(CancellationToken cancellationToken)
        => CheckIfTableExists(cancellationToken);

    public async Task Delete(string code, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(code))
        {
            throw new ArgumentException($"'{nameof(code)}' cannot be null or empty.", nameof(code));
        }

        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var codeStored = await GetCode(code, cancellationToken);

            if (codeStored == null)
            {
                return;
            }

            var tableEntity = codeStored.ToTableEntity();
            await Delete(tableEntity.PartitionKey, tableEntity.RowKey, cancellationToken: cancellationToken);
        }
        catch (Exception exception)
        {
            logger.CodesDeletingError(exception);
            throw;
        }
    }

    public async Task<int> DeleteExpired(DateTime expiration, CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var queryResult = Table.QueryAsync<AuthorizationCodeTableEntity>(a => a.Created <= expiration, cancellationToken: cancellationToken);
            var expiredCodes = new List<AuthorizationCodeTableEntity>();

            await queryResult.AsPages()
                .ForEachAsync(page => expiredCodes.AddRange(page.Values), cancellationToken)
                .ConfigureAwait(false);

            if (expiredCodes.Any())
            {
                expiredCodes.ToList().ForEach(async e => await Delete(e.PartitionKey, e.RowKey, cancellationToken));
                await AzureTableStorageDataContext.BatchManipulateEntities(Table
                    , expiredCodes
                    , TableTransactionActionType.Delete
                    , cancellationToken);
            }

            return expiredCodes.Count;
        }
        catch (Exception exception)
        {
            logger.CodesDeletingError(exception);
            throw;
        }
    }

    private Task Delete(string partitionKey, string rowKey, CancellationToken cancellationToken)
     => Table.DeleteEntityAsync(partitionKey, rowKey, cancellationToken: cancellationToken);


    public async Task<AuthorizationCode> GetCode(string code, CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var queryResult = Table.QueryAsync<AuthorizationCodeTableEntity>(a => a.RowKey == code, cancellationToken: cancellationToken);
            var results = new List<AuthorizationCodeTableEntity>();

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

            throw new DataAccessException($"Found duplicate codes ({results.Count}) for code {code}");
        }
        catch (Exception exception)
        {
            logger.CodesLoadingError(exception);
            throw;
        }
    }

    public async Task<AuthorizationCode> Create(AuthorizationCode code, CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var tableEntity = code.ToTableEntity();
            await Table.AddEntityAsync(tableEntity, cancellationToken);
            return code;
        }
        catch (Exception exception)
        {
            logger.CodesInsertingError(exception);
            throw;
        }
    }
}
