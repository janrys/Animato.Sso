namespace Animato.Sso.Application.Features.Claims;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Common.Logging;
using Animato.Sso.Application.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public class GetClaimByNameQuery : IRequest<Domain.Entities.Claim>
{
    public GetClaimByNameQuery(string name, ClaimsPrincipal user)
    {
        Name = name;
        User = user;
    }

    public string Name { get; }
    public ClaimsPrincipal User { get; }

    public class GetClaimByNameQueryValidator : AbstractValidator<GetClaimByNameQuery>
    {
        public GetClaimByNameQueryValidator()
            => RuleFor(v => v.Name).NotEmpty().WithMessage(v => $"{nameof(v.Name)} must have a value");

    }

    public class GetClaimByNameQueryHandler : IRequestHandler<GetClaimByNameQuery, Domain.Entities.Claim>
    {
        private readonly IClaimRepository claimRepository;
        private readonly ILogger<GetClaimByNameQueryHandler> logger;

        public GetClaimByNameQueryHandler(IClaimRepository claimRepository, ILogger<GetClaimByNameQueryHandler> logger)
        {
            this.claimRepository = claimRepository ?? throw new ArgumentNullException(nameof(claimRepository));
            this.logger = logger;
        }

        public async Task<Domain.Entities.Claim> Handle(GetClaimByNameQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return await claimRepository.GetClaimByName(request.Name, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.ClaimsLoadingError(exception);
                throw new DataAccessException(LogMessageTexts.ErrorLoadingClaims, exception);
            }
        }
    }
}
