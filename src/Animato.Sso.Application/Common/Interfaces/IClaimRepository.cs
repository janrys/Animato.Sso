namespace Animato.Sso.Application.Common.Interfaces;

using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Domain.Entities;

public interface IClaimRepository
{
    Task<Claim> Create(Claim claim, CancellationToken cancellationToken);
    Task<Claim> Create(Claim claim, ClaimId id, CancellationToken cancellationToken);
    Task<Claim> Update(Claim claim, CancellationToken cancellationToken);
    Task<IEnumerable<Claim>> GetClaimsByScope(string scopeName, int topCount, CancellationToken cancellationToken);
    Task<IEnumerable<Claim>> GetAll(CancellationToken cancellationToken);
    Task<Claim> GetClaimByName(string name, CancellationToken cancellationToken);
    Task<Claim> GetBydId(ClaimId id, CancellationToken cancellationToken);
    Task<IEnumerable<Claim>> GetBydId(CancellationToken cancellationToken, params ClaimId[] ids);
    Task Delete(string name, CancellationToken cancellationToken);
    Task DeleteScopesByScope(ScopeId scopeId, CancellationToken cancellationToken);
    Task DeleteScopesByClaim(ClaimId id, CancellationToken cancellationToken);
    Task DeleteUserClaims(ClaimId id, CancellationToken cancellationToken);
}
