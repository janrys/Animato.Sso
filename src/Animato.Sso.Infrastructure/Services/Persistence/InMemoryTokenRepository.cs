namespace Animato.Sso.Infrastructure.Services.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Domain.Entities;
using Microsoft.Extensions.Logging;

public class InMemoryTokenRepository : ITokenRepository
{
    private const string ERROR_LOADING_TOKENS = "Error loading tokens";
    private const string ERROR_INSERTING_TOKENS = "Error inserting token";
    private const string ERROR_DELETING_TOKENS = "Error deleting token";
    private const string ERROR_UPDATING_TOKENS = "Error updating token";
    private readonly InMemoryDataContext dataContext;
    private readonly ILogger<InMemoryTokenRepository> logger;

    public InMemoryTokenRepository(InMemoryDataContext dataContext, ILogger<InMemoryTokenRepository> logger)
    {
        this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<Token> GetToken(string token, CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(dataContext.Tokens.FirstOrDefault(t => t.Value == token));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_LOADING_TOKENS);
            throw;
        }
    }

    public async Task<Token> Insert(Token token, CancellationToken cancellationToken)
    {
        try
        {
            token.Id = new TokenId(Guid.NewGuid());
            dataContext.Tokens.Add(token);
            return (await InsertInternal(cancellationToken, token)).FirstOrDefault();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_INSERTING_TOKENS);
            throw;
        }
    }

    public Task<IEnumerable<Token>> Insert(Token accessToken, Token refreshToken, CancellationToken cancellationToken)
    {
        refreshToken.Id = new TokenId(Guid.NewGuid());
        accessToken.Id = new TokenId(Guid.NewGuid());
        accessToken.RefreshTokenId = refreshToken.Id;
        return InsertInternal(cancellationToken, accessToken, refreshToken);
    }

    public Task<IEnumerable<Token>> Insert(Token accessToken, Token refreshToken, Token idToken, CancellationToken cancellationToken)
    {
        refreshToken.Id = new TokenId(Guid.NewGuid());
        accessToken.Id = new TokenId(Guid.NewGuid());
        idToken.Id = new TokenId(Guid.NewGuid());
        accessToken.RefreshTokenId = refreshToken.Id;
        idToken.RefreshTokenId = refreshToken.Id;
        return InsertInternal(cancellationToken, accessToken, refreshToken, idToken);
    }

    private Task<IEnumerable<Token>> InsertInternal(CancellationToken cancellationToken, params Token[] tokens)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            dataContext.Tokens.AddRange(tokens);
            return Task.FromResult(tokens.AsEnumerable());
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_INSERTING_TOKENS);
            throw;
        }
    }

    public Task<int> RemoveExpiredTokens()
    {
        try
        {
            var removedTokens = dataContext.Tokens.RemoveAll(t => t.Expiration <= DateTime.UtcNow);
            return Task.FromResult(removedTokens);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_DELETING_TOKENS);
            throw;
        }
    }

    public Task Revoke(string token, CancellationToken cancellationToken)
    {
        try
        {
            var storedToken = dataContext.Tokens
                .FirstOrDefault(t => t.Value == token && !t.Revoked.HasValue);

            if (storedToken is not null)
            {
                storedToken.Revoked = DateTime.UtcNow;
            }

            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_UPDATING_TOKENS);
            throw;
        }
    }

    public Task RevokeTokensForUser(UserId id, CancellationToken cancellationToken)
    {
        try
        {
            var storedToken = dataContext.Tokens.Where(t => t.UserId == id && !t.Revoked.HasValue);

            if (storedToken.Any())
            {
                storedToken.ToList().ForEach(t => t.Revoked = DateTime.UtcNow);
            }

            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_UPDATING_TOKENS);
            throw;
        }
    }
}
