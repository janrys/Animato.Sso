namespace Animato.Sso.Application.Features.Claims;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Common.Logging;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Application.Features.Claims.DTOs;
using Animato.Sso.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public class UpdateClaimCommand : IRequest<Domain.Entities.Claim>
{
    public UpdateClaimCommand(string oldName, CreateClaimModel claim
        , ClaimsPrincipal user)
    {
        OldName = oldName;
        Claim = claim;
        User = user;
    }

    public string OldName { get; }
    public CreateClaimModel Claim { get; }
    public ClaimsPrincipal User { get; }

    public class UpdateClaimCommandValidator : AbstractValidator<UpdateClaimCommand>
    {
        public UpdateClaimCommandValidator()
        {
            RuleFor(v => v.OldName).NotEmpty().WithMessage(v => $"{nameof(v.OldName)} must have a value");
            RuleFor(v => v.Claim).NotNull().WithMessage(v => $"{nameof(v.Claim)} must have a value");
            RuleFor(v => v.Claim.Name).NotEmpty().WithMessage(v => $"{nameof(v.Claim.Name)} must have a value");
        }
    }

    public class UpdateClaimCommandHandler : IRequestHandler<UpdateClaimCommand, Domain.Entities.Claim>
    {
        private readonly IClaimRepository claimRepository;
        private readonly ILogger<UpdateClaimCommandHandler> logger;

        public UpdateClaimCommandHandler(IClaimRepository claimRepository
            , ILogger<UpdateClaimCommandHandler> logger)
        {
            this.claimRepository = claimRepository ?? throw new ArgumentNullException(nameof(claimRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Domain.Entities.Claim> Handle(UpdateClaimCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var claim = await claimRepository.GetClaimByName(request.OldName, cancellationToken);

                if (claim is null)
                {
                    throw new NotFoundException(nameof(Application), request.OldName);
                }

                if (!request.Claim.Name.Equals(request.OldName, StringComparison.OrdinalIgnoreCase))
                {
                    var targetClaim = await claimRepository.GetClaimByName(request.Claim.Name, cancellationToken);

                    if (targetClaim is not null)
                    {
                        throw new Exceptions.ValidationException(
                        Exceptions.ValidationException.CreateFailure("Name"
                        , $"Claim with name {request.Claim.Name} already exists"));
                    }
                }

                request.Claim.ValidateAndSanitize();
                claim = request.Claim.ApplyTo(claim);
                return await claimRepository.Update(claim, cancellationToken);
            }
            catch (NotFoundException) { throw; }
            catch (Exceptions.ValidationException) { throw; }
            catch (Exception exception)
            {
                logger.ClaimsUpdatingError(exception);
                throw new DataAccessException(LogMessageTexts.ErrorUpdatingClaims, exception);
            }
        }
    }

}
