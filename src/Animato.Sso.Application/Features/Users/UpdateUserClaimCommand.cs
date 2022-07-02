namespace Animato.Sso.Application.Features.Users;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Common.Logging;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Application.Features.Users.DTOs;
using Animato.Sso.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public class UpdateUserClaimCommand : IRequest<IEnumerable<UserClaim>>
{
    public UpdateUserClaimCommand(UserId userId
        , UserClaimId userClaimId
        , UpdateUserClaimModel claim
        , ClaimsPrincipal user)
    {
        UserId = userId;
        UserClaimId = userClaimId;
        Claim = claim;
        User = user;
    }

    public UserId UserId { get; }
    public UserClaimId UserClaimId { get; }
    public UpdateUserClaimModel Claim { get; }
    public ClaimsPrincipal User { get; }

    public class UpdateUserClaimCommandValidator : AbstractValidator<UpdateUserClaimCommand>
    {
        public UpdateUserClaimCommandValidator()
        {
            RuleFor(v => v.UserClaimId).NotNull().WithMessage(v => $"{nameof(v.UserClaimId)} must have a value");
            RuleFor(v => v.UserId).NotNull().WithMessage(v => $"{nameof(v.UserId)} must have a value");
            RuleFor(v => v.Claim).NotNull().WithMessage(v => $"{nameof(v.Claim)} must have a value");
        }
    }

    public class UpdateUserClaimCommandHandler : IRequestHandler<UpdateUserClaimCommand, IEnumerable<UserClaim>>
    {
        private readonly IUserRepository userRepository;
        private readonly IApplicationRoleRepository roleRepository;
        private readonly IClaimRepository claimRepository;
        private readonly ILogger<UpdateUserClaimCommandHandler> logger;

        public UpdateUserClaimCommandHandler(IUserRepository userRepository
            , IApplicationRoleRepository roleRepository
            , IClaimRepository claimRepository
            , ILogger<UpdateUserClaimCommandHandler> logger)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            this.claimRepository = claimRepository ?? throw new ArgumentNullException(nameof(claimRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<UserClaim>> Handle(UpdateUserClaimCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await userRepository.GetById(request.UserId, cancellationToken);

                if (user is null)
                {
                    throw new NotFoundException(nameof(User), request.UserId);
                }

                var userClaim = await userRepository.GetClaim(request.UserClaimId, cancellationToken);

                if (userClaim is null)
                {
                    throw new NotFoundException(nameof(UserClaim), request.UserClaimId);
                }

                userClaim.Value = request.Claim.Value;

                await userRepository.UpdateUserClaim(userClaim, cancellationToken);

                return await userRepository.GetClaims(user.Id, cancellationToken);
            }
            catch (NotFoundException) { throw; }
            catch (Exceptions.ValidationException) { throw; }
            catch (Exception exception)
            {
                logger.UsersUpdatingError(exception);
                throw new DataAccessException(LogMessageTexts.ErrorUpdatingUsers, exception);
            }
        }
    }

}
