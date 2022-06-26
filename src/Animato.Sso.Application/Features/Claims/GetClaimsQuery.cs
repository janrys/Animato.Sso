namespace Animato.Sso.Application.Features.Claims;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Common.Logging;
using Animato.Sso.Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

public class GetClaimsQuery : IRequest<IEnumerable<Domain.Entities.Claim>>
{
    public GetClaimsQuery(ClaimsPrincipal user) => User = user;

    public ClaimsPrincipal User { get; }

    public class GetClaimsQueryHandler : IRequestHandler<GetClaimsQuery, IEnumerable<Domain.Entities.Claim>>
    {
        private readonly IClaimRepository claimRepository;
        private readonly ILogger<GetClaimsQueryHandler> logger;

        public GetClaimsQueryHandler(IClaimRepository claimRepository, ILogger<GetClaimsQueryHandler> logger)
        {
            this.claimRepository = claimRepository ?? throw new ArgumentNullException(nameof(claimRepository));
            this.logger = logger;
        }

        public async Task<IEnumerable<Domain.Entities.Claim>> Handle(GetClaimsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return await claimRepository.GetAll(cancellationToken);
            }
            catch (Exception exception)
            {
                logger.ClaimsLoadingError(exception);
                throw new DataAccessException(LogMessageTexts.ErrorLoadingClaims, exception);
            }
        }
    }
}
