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
    private readonly ILogger<InMemoryClaimRepository> logger;

    public InMemoryClaimRepository(InMemoryDataContext dataContext
        , ILogger<InMemoryClaimRepository> logger)
    {
        if (dataContext is null)
        {
            throw new ArgumentNullException(nameof(dataContext));
        }

        claims = dataContext.Claims;
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
            logger.ScopesInsertingError(exception);
            throw;
        }
    }
}
