namespace Animato.Sso.Infrastructure.AzureStorage.Services.Persistence;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Domain.Entities;

public class AzureTableScopeRepository : IScopeRepository
{
    public Task<Scope> Create(Scope scope, CancellationToken cancellationToken)
        => Create(scope, ScopeId.New(), cancellationToken);

    public Task<Scope> Create(Scope scope, ScopeId id, CancellationToken cancellationToken)
        => Task.FromResult(scope);
}
