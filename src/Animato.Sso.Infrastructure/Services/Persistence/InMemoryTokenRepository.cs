namespace Animato.Sso.Infrastructure.Services.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Common.Logging;
using Animato.Sso.Domain.Entities;
using Microsoft.Extensions.Logging;

public class InMemoryTokenRepository : ITokenRepository
{
    private readonly List<Token> tokens;
    private readonly IDateTimeService dateTime;
    private readonly ILogger<InMemoryTokenRepository> logger;

    public InMemoryTokenRepository(InMemoryDataContext dataContext
        , IDateTimeService dateTime
        , ILogger<InMemoryTokenRepository> logger)
    {
        if (dataContext is null)
        {
            throw new ArgumentNullException(nameof(dataContext));
        }

        tokens = dataContext.Tokens;
        this.dateTime = dateTime ?? throw new ArgumentNullException(nameof(dateTime));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<Token> GetToken(string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(token))
        {
            throw new ArgumentException($"'{nameof(token)}' cannot be null or empty.", nameof(token));
        }

        try
        {
            return Task.FromResult(tokens.FirstOrDefault(t => t.Value == token));
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

        try
        {
            token.Id = new TokenId(Guid.NewGuid());
            return (await InsertInternal(cancellationToken, token)).FirstOrDefault();
        }
        catch (Exception exception)
        {
            logger.TokensInsertingError(exception);
            throw;
        }
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
        return InsertInternal(cancellationToken, accessToken, refreshToken);
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
        return InsertInternal(cancellationToken, accessToken, refreshToken, idToken);
    }

    private Task<IEnumerable<Token>> InsertInternal(CancellationToken cancellationToken, params Token[] tokens)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.tokens.AddRange(tokens);
            return Task.FromResult(tokens.AsEnumerable());
        }
        catch (Exception exception)
        {
            logger.TokensInsertingError(exception);
            throw;
        }
    }

    public Task<int> DeleteExpiredTokens(CancellationToken cancellationToken)
    {
        try
        {
            var removedTokens = tokens.RemoveAll(t => t.Expiration <= dateTime.UtcNow);
            return Task.FromResult(removedTokens);
        }
        catch (Exception exception)
        {
            logger.TokensDeletingError(exception);
            throw;
        }
    }

    public Task Revoke(string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(token))
        {
            throw new ArgumentException($"'{nameof(token)}' cannot be null or empty.", nameof(token));
        }

        try
        {
            var storedToken = tokens
                .FirstOrDefault(t => t.Value == token && !t.Revoked.HasValue);

            if (storedToken is not null)
            {
                storedToken.Revoked = dateTime.UtcNow;
            }

            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            logger.TokensUpdatingError(exception);
            throw;
        }
    }

    public Task RevokeTokensForUser(UserId id, CancellationToken cancellationToken)
    {
        try
        {
            var storedTokens = tokens.Where(t => t.UserId == id && !t.Revoked.HasValue);

            if (storedTokens.Any())
            {
                storedTokens.ToList().ForEach(t => t.Revoked = dateTime.UtcNow);
            }

            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            logger.TokensUpdatingError(exception);
            throw;
        }
    }
}
