namespace Animato.Sso.Application.Features.Scopes;
using System;
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

public class RemoveClaimScopeCommand : IRequest<IEnumerable<Domain.Entities.Claim>>
{
    public RemoveClaimScopeCommand(string scopeName, string claimName
        , ClaimsPrincipal user)
    {
        ScopeName = scopeName;
        ClaimName = claimName;
        User = user;
    }

    public string ScopeName { get; }
    public string ClaimName { get; }

    public ClaimsPrincipal User { get; }

    public class RemoveClaimScopeCommandValidator : AbstractValidator<RemoveClaimScopeCommand>
    {
        public RemoveClaimScopeCommandValidator()
        {
            RuleFor(v => v.ScopeName).NotEmpty().WithMessage(v => $"{nameof(v.ScopeName)} must have a value");
            RuleFor(v => v.ClaimName).NotEmpty().WithMessage(v => $"{nameof(v.ClaimName)} must have a value");
        }
    }

    public class RemoveClaimScopeCommandHandler : IRequestHandler<RemoveClaimScopeCommand, IEnumerable<Domain.Entities.Claim>>
    {
        private readonly IScopeRepository scopeRepository;
        private readonly IClaimRepository claimRepository;
        private readonly ILogger<RemoveClaimScopeCommandHandler> logger;

        public RemoveClaimScopeCommandHandler(IScopeRepository scopeRepository
            , IClaimRepository claimRepository
            , ILogger<RemoveClaimScopeCommandHandler> logger)
        {
            this.scopeRepository = scopeRepository ?? throw new ArgumentNullException(nameof(scopeRepository));
            this.claimRepository = claimRepository;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<Domain.Entities.Claim>> Handle(RemoveClaimScopeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var scope = await scopeRepository.GetScopeByName(request.ScopeName, cancellationToken);
                if (scope == null)
                {
                    throw new NotFoundException(nameof(Scope), request.ScopeName);
                }

                var claim = await claimRepository.GetClaimByName(request.ClaimName, cancellationToken);
                if (claim == null)
                {
                    throw new NotFoundException(nameof(Domain.Entities.Claim), request.ClaimName);
                }

                await claimRepository.RemoveClaimScope(scope.Id, claim.Id, cancellationToken);
                return await claimRepository.GetByScope(scope.Id, cancellationToken);
            }
            catch (NotFoundException) { throw; }
            catch (Exceptions.ValidationException) { throw; }
            catch (Exception exception)
            {
                logger.ScopesUpdatingError(exception);
                throw new DataAccessException(LogMessageTexts.ErrorUpdatingScopes, exception);
            }
        }
    }

}
