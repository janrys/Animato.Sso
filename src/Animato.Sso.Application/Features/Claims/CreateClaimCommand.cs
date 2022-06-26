namespace Animato.Sso.Application.Features.Claims;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Common.Logging;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Application.Features.Claims.DTOs;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public class CreateClaimCommand : IRequest<Domain.Entities.Claim>
{
    public CreateClaimCommand(CreateClaimModel claim, ClaimsPrincipal user)
    {
        Claim = claim;
        User = user;
    }

    public CreateClaimModel Claim { get; }
    public ClaimsPrincipal User { get; }

    public class CreateClaimCommandValidator : AbstractValidator<CreateClaimCommand>
    {
        public CreateClaimCommandValidator()
        {
            RuleFor(v => v.Claim).NotNull().WithMessage(v => $"{nameof(v.Claim)} must have a value");
            RuleFor(v => v.Claim.Name).NotEmpty().WithMessage(v => $"{nameof(v.Claim.Name)} must have a value");
        }
    }

    public class CreateClaimCommandHandler : IRequestHandler<CreateClaimCommand, Domain.Entities.Claim>
    {
        private readonly IClaimRepository claimRepository;
        private readonly ILogger<CreateClaimCommandHandler> logger;

        public CreateClaimCommandHandler(IClaimRepository claimRepository
            , ILogger<CreateClaimCommandHandler> logger)
        {
            this.claimRepository = claimRepository ?? throw new ArgumentNullException(nameof(claimRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Domain.Entities.Claim> Handle(CreateClaimCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var claim = await claimRepository.GetClaimByName(request.Claim.Name, cancellationToken);

                if (claim is not null)
                {
                    throw new Exceptions.ValidationException(
                        Exceptions.ValidationException.CreateFailure("name"
                        , $"Claim with name {request.Claim.Name} already exists"));
                }

                request.Claim.ValidateAndSanitize();
                claim = new Domain.Entities.Claim();
                claim = request.Claim.ApplyTo(claim);
                return await claimRepository.Create(claim, cancellationToken);
            }
            catch (Exceptions.ValidationException) { throw; }
            catch (Exception exception)
            {
                logger.ClaimsCreatingError(exception);
                throw new DataAccessException(LogMessageTexts.ErrorCreatingClaims, exception);
            }
        }
    }

}

