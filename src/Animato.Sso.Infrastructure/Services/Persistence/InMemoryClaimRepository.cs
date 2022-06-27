namespace Animato.Sso.Infrastructure.Services.Persistence;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Common.Logging;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Domain.Entities;
using Microsoft.Extensions.Logging;

public class InMemoryClaimRepository : IClaimRepository
{
    private readonly List<Claim> claims;
    private readonly List<ClaimScope> claimScopes;
    private readonly List<Scope> scopes;
    private readonly List<UserClaim> userClaims;
    private readonly ILogger<InMemoryClaimRepository> logger;

    public InMemoryClaimRepository(InMemoryDataContext dataContext
        , ILogger<InMemoryClaimRepository> logger)
    {
        if (dataContext is null)
        {
            throw new ArgumentNullException(nameof(dataContext));
        }

        claims = dataContext.Claims;
        claimScopes = dataContext.ClaimScopes;
        scopes = dataContext.Scopes;
        userClaims = dataContext.UserClaims;
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<Claim> Create(Claim claim, CancellationToken cancellationToken)
        => Create(claim, ClaimId.New(), cancellationToken);

    public Task<Claim> Create(Claim claim, ClaimId id, CancellationToken cancellationToken)
    {
        if (claim is null)
        {
            throw new ArgumentNullException(nameof(claim));
        }

        claim.Id = id;

        try
        {
            claims.Add(claim);
            return Task.FromResult(claim);
        }
        catch (Exception exception)
        {
            logger.ScopesCreatingError(exception);
            throw;
        }
    }

    public Task<IEnumerable<Claim>> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(claims.AsEnumerable());
        }
        catch (Exception exception)
        {
            logger.ClaimsLoadingError(exception);
            throw;
        }
    }

    public Task<Claim> GetClaimByName(string name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
        }

        try
        {
            return Task.FromResult(claims.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)));
        }
        catch (Exception exception)
        {
            logger.ClaimsLoadingError(exception);
            throw;
        }
    }

    public Task<Claim> GetBydId(ClaimId id, CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(claims.FirstOrDefault(x => x.Id == id));
        }
        catch (Exception exception)
        {
            logger.ClaimsLoadingError(exception);
            throw;
        }
    }

    public Task<IEnumerable<Claim>> GetBydId(CancellationToken cancellationToken, params ClaimId[] ids)
    {
        try
        {
            return Task.FromResult(claims.Where(x => ids.Any(id => x.Id == id)));
        }
        catch (Exception exception)
        {
            logger.ClaimsLoadingError(exception);
            throw;
        }
    }

    public Task<IEnumerable<Claim>> GetClaimsByScope(string scopeName, int topCount, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(scopeName))
        {
            throw new ArgumentException($"'{nameof(scopeName)}' cannot be null or empty.", nameof(scopeName));
        }

        if (topCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(topCount), "Must be greater than 0");
        }

        try
        {
            return Task.FromResult(scopes.Where(s => s.Name.Equals(scopeName, StringComparison.OrdinalIgnoreCase))
                .Join(claimScopes, scope => scope.Id, cs => cs.ScopeId, (scope, cs) => cs)
                .Join(claims, cs => cs.ClaimId, c => c.Id, (cs, c) => c)
                .Take(topCount));
        }
        catch (Exception exception)
        {
            logger.ClaimsLoadingError(exception);
            throw;
        }
    }
    public async Task<Claim> Update(Claim claim, CancellationToken cancellationToken)
    {
        try
        {
            var storedClaim = await GetBydId(claim.Id, cancellationToken);

            if (storedClaim == null)
            {
                throw new NotFoundException(nameof(Claim), claim.Id);
            }

            claims.RemoveAll(s => s.Id == claim.Id);
            claims.Add(claim);

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

        try
        {
            var claim = await GetClaimByName(name, cancellationToken);

            if (claim is null)
            {
                return;
            }

            await DeleteScopesByClaim(claim.Id, cancellationToken);
            await DeleteUserClaims(claim.Id, cancellationToken);
            claims.RemoveAll(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception exception)
        {
            logger.ClaimsDeletingError(exception);
            throw;
        }
    }

    public Task DeleteUserClaims(ClaimId id, CancellationToken cancellationToken)
    {
        try
        {
            userClaims.RemoveAll(c => c.ClaimId == id);
            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            logger.ClaimsDeletingError(exception);
            throw;
        }
    }

    public Task DeleteScopesByClaim(ClaimId id, CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(claimScopes.RemoveAll(s => s.ClaimId == id));
        }
        catch (Exception exception)
        {
            logger.ClaimsDeletingError(exception);
            throw;
        }
    }

    public Task DeleteScopesByScope(ScopeId scopeId, CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(claimScopes.RemoveAll(s => s.ScopeId == scopeId));
        }
        catch (Exception exception)
        {
            logger.ClaimsDeletingError(exception);
            throw;
        }
    }
}
