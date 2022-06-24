namespace Animato.Sso.Infrastructure.Services.Persistence;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Common.Logging;
using Animato.Sso.Domain.Entities;
using Microsoft.Extensions.Logging;

public class InMemoryClaimRepository : IClaimRepository
{
    private readonly List<Claim> claims;
    private readonly List<ClaimScope> claimScopes;
    private readonly List<Scope> scopes;
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
            cancellationToken.ThrowIfCancellationRequested();
            claims.Add(claim);
            return Task.FromResult(claim);
        }
        catch (Exception exception)
        {
            logger.ScopesCreatingError(exception);
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
}
