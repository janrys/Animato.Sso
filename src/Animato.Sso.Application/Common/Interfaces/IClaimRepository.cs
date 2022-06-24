namespace Animato.Sso.Application.Common.Interfaces;

using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Domain.Entities;

public interface IClaimRepository
{
    Task<Claim> Create(Claim claim, CancellationToken cancellationToken);
    Task<Claim> Create(Claim claim, ClaimId id, CancellationToken cancellationToken);
    Task<IEnumerable<Claim>> GetClaimsByScope(string scopeName, int topCount, CancellationToken cancellationToken);
}
