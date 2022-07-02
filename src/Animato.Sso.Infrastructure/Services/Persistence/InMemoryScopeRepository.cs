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
            logger.ClaimsCreatingError(exception);
            throw;
        }
    }

    public Task<Scope> GetScopeByName(string name, CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(scopes.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)));
        }
        catch (Exception exception)
        {
            logger.ScopesLoadingError(exception);
            throw;
        }
    }

    public Task<IEnumerable<Scope>> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(scopes.AsEnumerable());
        }
        catch (Exception exception)
        {
            logger.ScopesLoadingError(exception);
            throw;
        }
    }

    public Task<IEnumerable<Scope>> GetScopesByName(CancellationToken cancellationToken, params string[] names)
    {
        if (names is null)
        {
            throw new ArgumentNullException(nameof(names));
        }

        return Task.FromResult(scopes.Where(s => names.Any(n => s.Name.Equals(n, StringComparison.OrdinalIgnoreCase))));
    }

    public Task<IEnumerable<Scope>> GetScopesById(CancellationToken cancellationToken, params ScopeId[] ids)
    {
        if (ids is null)
        {
            throw new ArgumentNullException(nameof(ids));
        }

        return Task.FromResult(scopes.Where(s => ids.Any(id => s.Id == id)));
    }

    public Task<IEnumerable<Scope>> Create(CancellationToken cancellationToken, params Scope[] scopes)
    {
        try
        {
            scopes.ToList().ForEach(s => s.Id = ScopeId.New());
            this.scopes.AddRange(scopes);
            return Task.FromResult(scopes.AsEnumerable());
        }
        catch (Exception exception)
        {
            logger.ScopesCreatingError(exception);
            throw;
        }
    }

    public async Task<Scope> Update(string oldName, string newName, CancellationToken cancellationToken)
    {
        try
        {
            var scope = await GetScopeByName(oldName, cancellationToken);

            if (scope == null)
            {
                throw new NotFoundException(nameof(Scope), oldName);
            }

            scope.Name = newName;

            return scope;
        }
        catch (Exception exception)
        {
            logger.ScopesUpdatingError(exception);
            throw;
        }
    }

    public Task Delete(string name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
        }

        try
        {
            return Task.FromResult(scopes.RemoveAll(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)));
        }
        catch (Exception exception)
        {
            logger.ScopesDeletingError(exception);
            throw;
        }
    }

    public Task Clear(CancellationToken cancellationToken)
    {
        scopes.Clear();
        return Task.CompletedTask;
    }


}
