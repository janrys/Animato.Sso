namespace Animato.Sso.Infrastructure.Services.Persistence;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Common.Logging;
using Animato.Sso.Domain.Entities;
using Microsoft.Extensions.Logging;

public class InMemoryScopeRepository : IScopeRepository
{
    private readonly List<Scope> scopes;
    private readonly ILogger<InMemoryScopeRepository> logger;

    public InMemoryScopeRepository(InMemoryDataContext dataContext
        , ILogger<InMemoryScopeRepository> logger)
    {
        if (dataContext is null)
        {
            throw new ArgumentNullException(nameof(dataContext));
        }

        scopes = dataContext.Scopes;
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<Scope> Create(Scope scope, CancellationToken cancellationToken)
        => Create(scope, ScopeId.New(), cancellationToken);

    public Task<Scope> Create(Scope scope, ScopeId id, CancellationToken cancellationToken)
    {
        if (scope is null)
        {
            throw new ArgumentNullException(nameof(scope));
        }

        scope.Id = id;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            scopes.Add(scope);
            return Task.FromResult(scope);
        }
        catch (Exception exception)
        {
            logger.ClaimsInsertingError(exception);
            throw;
        }
    }
}
