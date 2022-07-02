namespace Animato.Sso.Application.Features.Users;
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

public class RemoveUserClaimCommand : IRequest<IEnumerable<UserClaim>>
{
    public RemoveUserClaimCommand(UserId userId
        , UserClaimId userClaimId
        , ClaimsPrincipal user)
    {
        UserId = userId;
        UserClaimId = userClaimId;
        User = user;
    }

    public UserId UserId { get; }
    public UserClaimId UserClaimId { get; }
    public ClaimsPrincipal User { get; }

    public class RemoveUserClaimCommandValidator : AbstractValidator<RemoveUserClaimCommand>
    {
        public RemoveUserClaimCommandValidator()
        {
            RuleFor(v => v.UserClaimId).NotNull().WithMessage(v => $"{nameof(v.UserClaimId)} must have a value");
            RuleFor(v => v.UserId).NotNull().WithMessage(v => $"{nameof(v.UserId)} must have a value");
        }
    }

    public class RemoveUserClaimCommandHandler : IRequestHandler<RemoveUserClaimCommand, IEnumerable<UserClaim>>
    {
        private readonly IUserRepository userRepository;
        private readonly ILogger<RemoveUserClaimCommandHandler> logger;

        public RemoveUserClaimCommandHandler(IUserRepository userRepository
            , ILogger<RemoveUserClaimCommandHandler> logger)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<UserClaim>> Handle(RemoveUserClaimCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await userRepository.GetById(request.UserId, cancellationToken);

                if (user is null)
                {
                    throw new NotFoundException(nameof(User), request.UserId);
                }

                await userRepository.RemoveUserClaim(request.UserClaimId, cancellationToken);

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
