namespace Animato.Sso.Application.Features.Users;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public class DeleteUserCommand : IRequest<Unit>
{
    public DeleteUserCommand(UserId userId, ClaimsPrincipal user)
    {
        UserId = userId;
        User = user;
    }

    public UserId UserId { get; }
    public ClaimsPrincipal User { get; }

    public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
    {
        public DeleteUserCommandValidator()
            => RuleFor(v => v.UserId).NotNull().WithMessage(v => $"{nameof(v.UserId)} must have a value");
    }

    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Unit>
    {
        private readonly IUserRepository userRepository;
        private readonly ILogger<DeleteUserCommandHandler> logger;
        private const string ERROR_DELETING_USER = "Error deleting user";

        public DeleteUserCommandHandler(IUserRepository userRepository
            , ILogger<DeleteUserCommandHandler> logger)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await userRepository.GetById(request.UserId, cancellationToken);

                if (user is not null && !user.IsDeleted)
                {
                    await userRepository.DeleteSoft(request.UserId, cancellationToken);
                }

                return Unit.Value;
            }
            catch (Exceptions.ValidationException) { throw; }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_DELETING_USER);
                throw new DataAccessException(ERROR_DELETING_USER, exception);
            }
        }
    }

}
