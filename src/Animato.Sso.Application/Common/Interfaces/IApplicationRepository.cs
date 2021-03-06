namespace Animato.Sso.Application.Common.Interfaces;

using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Domain.Entities;

public interface IApplicationRepository
{
    Task<IEnumerable<Application>> GetAll(CancellationToken cancellationToken);
    Task<Application> GetByCode(string code, CancellationToken cancellationToken);
    Task<Application> GetById(ApplicationId applicationId, CancellationToken cancellationToken);
    Task<Application> Create(Application application, CancellationToken cancellationToken);
    Task<Application> Update(Application application, CancellationToken cancellationToken);
    Task Delete(ApplicationId applicationId, CancellationToken cancellationToken);
    Task Clear(CancellationToken cancellationToken);
    Task DeleteApplicationScope(ScopeId scopeId, CancellationToken cancellationToken);
    Task DeleteApplicationScope(ApplicationId applicationId, ScopeId scopeId, CancellationToken cancellationToken);
    Task CreateApplicationScopes(ApplicationId applicationId, CancellationToken cancellationToken, params ScopeId[] scopes);
    Task<IEnumerable<Scope>> GetScopes(ApplicationId applicationId, CancellationToken cancellationToken);
}
