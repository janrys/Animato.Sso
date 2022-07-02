namespace Animato.Sso.Application.Features.Scopes;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Common.Logging;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public class GetClaimsByScopeQuery : IRequest<IEnumerable<Domain.Entities.Claim>>
{
    public GetClaimsByScopeQuery(string scopeName, ClaimsPrincipal user)
    {
        ScopeName = scopeName;
        User = user;
    }

    public string ScopeName { get; }
    public ClaimsPrincipal User { get; }

    public class GetClaimsByScopeQueryValidator : AbstractValidator<GetClaimsByScopeQuery>
    {
        public GetClaimsByScopeQueryValidator() => RuleFor(v => v.ScopeName).NotEmpty().WithMessage(v => $"{nameof(v.ScopeName)} must have a value");
    }

    public class GetClaimsByScopeQueryHandler : IRequestHandler<GetClaimsByScopeQuery, IEnumerable<Domain.Entities.Claim>>
    {
        private readonly IScopeRepository scopeRepository;
        private readonly IClaimRepository claimRepository;
        private readonly ILogger<GetClaimsByScopeQueryHandler> logger;

        public GetClaimsByScopeQueryHandler(IScopeRepository scopeRepository
            , IClaimRepository claimRepository
            , ILogger<GetClaimsByScopeQueryHandler> logger)
        {
            this.scopeRepository = scopeRepository ?? throw new ArgumentNullException(nameof(scopeRepository));
            this.claimRepository = claimRepository ?? throw new ArgumentNullException(nameof(claimRepository));
            this.logger = logger;
        }

        public async Task<IEnumerable<Domain.Entities.Claim>> Handle(GetClaimsByScopeQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var scope = await scopeRepository.GetScopeByName(request.ScopeName, cancellationToken);

                if (scope is null)
                {
                    throw new NotFoundException(nameof(Scope), request.ScopeName);
                }

                return await claimRepository.GetByScope(request.ScopeName, cancellationToken);
            }
            catch (NotFoundException) { throw; }
            catch (Exception exception)
            {
                logger.ScopesLoadingError(exception);
                throw new DataAccessException(LogMessageTexts.ErrorLoadingScopes, exception);
            }
        }
    }
}
