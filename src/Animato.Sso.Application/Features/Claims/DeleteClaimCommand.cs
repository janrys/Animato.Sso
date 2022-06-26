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

public class DeleteClaimCommand : IRequest<Unit>
{
    public DeleteClaimCommand(string name, ClaimsPrincipal user)
    {
        Name = name;
        User = user;
    }

    public string Name { get; }
    public ClaimsPrincipal User { get; }

    public class DeleteClaimCommandValidator : AbstractValidator<DeleteClaimCommand>
    {
        public DeleteClaimCommandValidator()
            => RuleFor(v => v.Name).NotEmpty().WithMessage(v => $"{nameof(v.Name)} must have a value");
    }

    public class DeleteClaimCommandHandler : IRequestHandler<DeleteClaimCommand, Unit>
    {
        private readonly IClaimRepository claimRepository;
        private readonly IUserRepository userRepository;
        private readonly ILogger<DeleteClaimCommandHandler> logger;

        public DeleteClaimCommandHandler(IClaimRepository claimRepository
            , IUserRepository userRepository
            , ILogger<DeleteClaimCommandHandler> logger)
        {
            this.claimRepository = claimRepository ?? throw new ArgumentNullException(nameof(claimRepository));
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(DeleteClaimCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var claim = await claimRepository.GetClaimByName(request.Name, cancellationToken);

                if (claim is null)
                {
                    return Unit.Value;
                }

                var claims = await userRepository.GetClaims(claim.Id, 1, cancellationToken);
                if (claims.Any())
                {
                    throw new Exceptions.ValidationException(
                        Exceptions.ValidationException.CreateFailure("Claim", $"Claim is used by users.")
                        );
                }

                await claimRepository.RemoveScopesByClaim(claim.Id, cancellationToken);
                await claimRepository.Delete(claim.Name, cancellationToken);
                return Unit.Value;
            }
            catch (Exceptions.ValidationException) { throw; }
            catch (Exception exception)
            {
                logger.ScopesDeletingError(exception);
                throw new DataAccessException(LogMessageTexts.ErrorDeletingScopes, exception);
            }
        }
    }

}
