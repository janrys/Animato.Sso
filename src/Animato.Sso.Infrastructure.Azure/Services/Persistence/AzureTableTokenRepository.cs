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

public class AzureTableTokenRepository : ITokenRepository
{
    private TableClient Table => dataContext.Tokens;
    private Func<CancellationToken, Task> CheckIfTableExists => dataContext.ThrowExceptionIfTableNotExists;
    private readonly AzureTableStorageDataContext dataContext;
    private readonly ILogger<AzureTableTokenRepository> logger;

    public AzureTableTokenRepository(AzureTableStorageDataContext dataContext, ILogger<AzureTableTokenRepository> logger)
    {
        this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private Task ThrowExceptionIfTableNotExists(CancellationToken cancellationToken)
        => CheckIfTableExists(cancellationToken);

    public async Task<Token> GetToken(string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(token))
        {
            throw new ArgumentException($"'{nameof(token)}' cannot be null or empty.", nameof(token));
        }

        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var results = new List<TokenTableEntity>();
            var queryResult = Table.QueryAsync<TokenTableEntity>(a => a.Value == token, cancellationToken: cancellationToken);

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

            throw new DataAccessException($"Found duplicate tokens ({results.Count}) for token {token}");
        }
        catch (Exception exception)
        {
            logger.TokensLoadingError(exception);
            throw;
        }
    }

    public async Task<Token> Create(Token token, CancellationToken cancellationToken)
    {
        if (token is null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        token.Id = new TokenId(Guid.NewGuid());
        return (await CreateInternal(cancellationToken, token)).FirstOrDefault();
    }

    public Task<IEnumerable<Token>> Create(Token accessToken, Token refreshToken, CancellationToken cancellationToken)
    {
        if (accessToken is null)
        {
            throw new ArgumentNullException(nameof(accessToken));
        }

        if (refreshToken is null)
        {
            throw new ArgumentNullException(nameof(refreshToken));
        }

        refreshToken.Id = new TokenId(Guid.NewGuid());
        accessToken.Id = new TokenId(Guid.NewGuid());
        accessToken.RefreshTokenId = refreshToken.Id;
        return CreateInternal(cancellationToken, accessToken, refreshToken);
    }

    public Task<IEnumerable<Token>> Create(Token accessToken, Token refreshToken, Token idToken, CancellationToken cancellationToken)
    {
        if (accessToken is null)
        {
            throw new ArgumentNullException(nameof(accessToken));
        }

        if (refreshToken is null)
        {
            throw new ArgumentNullException(nameof(refreshToken));
        }

        if (idToken is null)
        {
            throw new ArgumentNullException(nameof(idToken));
        }

        refreshToken.Id = new TokenId(Guid.NewGuid());
        accessToken.Id = new TokenId(Guid.NewGuid());
        idToken.Id = new TokenId(Guid.NewGuid());
        accessToken.RefreshTokenId = refreshToken.Id;
        idToken.RefreshTokenId = refreshToken.Id;
        return CreateInternal(cancellationToken, accessToken, refreshToken, idToken);
    }

    private async Task<IEnumerable<Token>> CreateInternal(CancellationToken cancellationToken, params Token[] tokens)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            await AzureTableStorageDataContext.BatchManipulateEntities(Table
                , tokens.Select(t => t.ToTableEntity())
                , TableTransactionActionType.Add
                , cancellationToken);
            return tokens;
        }
        catch (Exception exception)
        {
            logger.TokensInsertingError(exception);
            throw;
        }
    }

    public async Task<int> DeleteExpiredTokens(CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var storedTokens = new List<TokenTableEntity>();
            var queryResult = Table.QueryAsync<TokenTableEntity>(t => t.Expiration <= DateTime.UtcNow, cancellationToken: cancellationToken);

            await queryResult.AsPages()
                .ForEachAsync(page => storedTokens.AddRange(page.Values), cancellationToken)
                .ConfigureAwait(false);

            if (storedTokens.Any())
            {
                await AzureTableStorageDataContext.BatchManipulateEntities(Table
                    , storedTokens
                    , TableTransactionActionType.Delete
                    , cancellationToken);
            }

            return storedTokens.Count;
        }
        catch (Exception exception)
        {
            logger.TokensDeletingError(exception);
            throw;
        }
    }

    public async Task Revoke(string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(token))
        {
            throw new ArgumentException($"'{nameof(token)}' cannot be null or empty.", nameof(token));
        }

        var storedToken = await GetToken(token, cancellationToken);

        if (storedToken is not null)
        {
            storedToken.Revoked = DateTime.UtcNow;
        }

        await UpdateToken(storedToken, cancellationToken);
    }

    private async Task<Token> UpdateToken(Token token, CancellationToken cancellationToken)
    {
        if (token is null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var tableEntity = token.ToTableEntity();
            await Table.UpdateEntityAsync(tableEntity, Azure.ETag.All, cancellationToken: cancellationToken);
            return token;
        }
        catch (Exception exception)
        {
            logger.TokensUpdatingError(exception);
            throw;
        }
    }

    public async Task RevokeTokensForUser(UserId id, CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var storedTokens = new List<TokenTableEntity>();
            var queryResult = Table.QueryAsync<TokenTableEntity>(a => a.PartitionKey == id.ToString()
                    && a.Revoked == null, cancellationToken: cancellationToken);

            await queryResult.AsPages()
                .ForEachAsync(page => storedTokens.AddRange(page.Values), cancellationToken)
                .ConfigureAwait(false);

            if (storedTokens.Any())
            {
                storedTokens.ToList().ForEach(t => t.Revoked = DateTime.UtcNow);
            }

            await AzureTableStorageDataContext.BatchManipulateEntities(Table
                , storedTokens
                , TableTransactionActionType.UpdateReplace
                , cancellationToken);
        }
        catch (Exception exception)
        {
            logger.TokensUpdatingError(exception);
            throw;
        }
    }
}
