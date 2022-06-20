namespace Animato.Sso.Infrastructure.AzureStorage.Services.Persistence;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Domain.Entities;

public class AzureTableClaimRepository : IClaimRepository
{
    public Task<Claim> Create(Claim claim, CancellationToken cancellationToken)
        => Create(claim, ClaimId.New(), cancellationToken);

    public Task<Claim> Create(Claim claim, ClaimId id, CancellationToken cancellationToken)
        => Task.FromResult(claim);
}
