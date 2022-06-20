namespace Animato.Sso.Application.Common.Interfaces;

using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Domain.Entities;

public interface IScopeRepository
{
    Task<Scope> Create(Scope scope, CancellationToken cancellationToken);
    Task<Scope> Create(Scope scope, ScopeId id, CancellationToken cancellationToken);
}
