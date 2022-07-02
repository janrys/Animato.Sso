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

public class AddUserClaimCommand : IRequest<IEnumerable<UserClaim>>
{
    public AddUserClaimCommand(UserId userId
        , AddUserClaimsModel claims
        , ClaimsPrincipal user)
    {
        UserId = userId;
        Claims = claims;
        User = user;
    }

    public UserId UserId { get; }
    public AddUserClaimsModel Claims { get; }
    public ClaimsPrincipal User { get; }

    public class AddUserClaimCommandValidator : AbstractValidator<AddUserClaimCommand>
    {
        public AddUserClaimCommandValidator()
        {
            RuleFor(v => v.Claims).NotNull().WithMessage(v => $"{nameof(v.Claims)} must have a value");
            RuleFor(v => v.Claims.Claims).NotNull().WithMessage(v => $"{nameof(v.Claims.Claims)} must have a value");
            RuleFor(v => v.Claims.Claims).Must(c => c.All(claim => !string.IsNullOrEmpty(claim.Name))).WithMessage(v => $"{nameof(v.Claims.Claims)} must have a value");
            RuleFor(v => v.UserId).NotNull().WithMessage(v => $"{nameof(v.UserId)} must have a value");
        }
    }

    public class AddUserClaimCommandHandler : IRequestHandler<AddUserClaimCommand, IEnumerable<UserClaim>>
    {
        private readonly IUserRepository userRepository;
        private readonly IApplicationRoleRepository roleRepository;
        private readonly IClaimRepository claimRepository;
        private readonly ILogger<AddUserClaimCommandHandler> logger;

        public AddUserClaimCommandHandler(IUserRepository userRepository
            , IApplicationRoleRepository roleRepository
            , IClaimRepository claimRepository
            , ILogger<AddUserClaimCommandHandler> logger)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            this.claimRepository = claimRepository ?? throw new ArgumentNullException(nameof(claimRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<UserClaim>> Handle(AddUserClaimCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await userRepository.GetById(request.UserId, cancellationToken);

                if (user is null)
                {
                    throw new NotFoundException(nameof(User), request.UserId);
                }

                var claims = await claimRepository.GetByName(cancellationToken, request.Claims.Claims.Select(c => c.Name).ToArray());

                var userClaims = new List<UserClaim>();
                foreach (var claim in request.Claims.Claims)
                {
                    var storedClaim = claims.FirstOrDefault(c => c.Name.Equals(claim.Name, StringComparison.OrdinalIgnoreCase));

                    if (storedClaim is null)
                    {
                        throw new NotFoundException(nameof(Domain.Entities.Claim), claim.Name);
                    }

                    var userClaim = new UserClaim
                    {
                        Value = claim.Value,
                        UserId = user.Id,
                        ClaimId = storedClaim.Id
                    };
                    userClaims.Add(userClaim);
                }

                await userRepository.AddUserClaims(user.Id, cancellationToken, userClaims.ToArray());

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
