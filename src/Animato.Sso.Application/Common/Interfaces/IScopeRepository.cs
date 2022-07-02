namespace Animato.Sso.Application.Common.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Domain.Entities;

public interface IScopeRepository
{
    Task<Scope> Create(Scope scope, CancellationToken cancellationToken);
    Task<Scope> Create(Scope scope, ScopeId id, CancellationToken cancellationToken);
    Task<IEnumerable<Scope>> Create(CancellationToken cancellationToken, params Scope[] scopes);
    Task<IEnumerable<Scope>> GetAll(CancellationToken cancellationToken);
    Task<Scope> GetScopeByName(string name, CancellationToken cancellationToken);
    Task<IEnumerable<Scope>> GetScopesByName(CancellationToken cancellationToken, params string[] names);
    Task<IEnumerable<Scope>> GetScopesById(CancellationToken cancellationToken, params ScopeId[] ids);
    Task Delete(string name, CancellationToken cancellationToken);
    Task<Scope> Update(string oldName, string newName, CancellationToken cancellationToken);
    Task Clear(CancellationToken cancellationToken);
}
