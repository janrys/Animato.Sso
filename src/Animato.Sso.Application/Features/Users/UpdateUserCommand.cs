namespace Animato.Sso.Application.Features.Users;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Application.Features.Applications.DTOs;
using Animato.Sso.Application.Features.Users.DTOs;
using Animato.Sso.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public class UpdateUserCommand : IRequest<User>
{
    public UpdateUserCommand(UserId userId
        , CreateUserModel userModel
        , ClaimsPrincipal user)
    {
        UserId = userId;
        UserModel = userModel;
        User = user;
    }

    public UserId UserId { get; }
    public CreateUserModel UserModel { get; }
    public ClaimsPrincipal User { get; }

    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(v => v.UserModel).NotNull().WithMessage(v => $"{nameof(v.UserModel)} must have a value");
            RuleFor(v => v.UserId).NotNull().WithMessage(v => $"{nameof(v.UserId)} must have a value");
        }
    }

    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, User>
    {
        private readonly OidcOptions oidcOptions;
        private readonly IUserRepository userRepository;
        private readonly ITokenFactory tokenFactory;
        private readonly ILogger<UpdateUserCommandHandler> logger;
        private const string ERROR_UPDATING_USER = "Error updating user";

        public UpdateUserCommandHandler(OidcOptions oidcOptions
            , IUserRepository userRepository
            , ITokenFactory tokenFactory
            , ILogger<UpdateUserCommandHandler> logger)
        {
            this.oidcOptions = oidcOptions ?? throw new ArgumentNullException(nameof(oidcOptions));
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.tokenFactory = tokenFactory ?? throw new ArgumentNullException(nameof(tokenFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<User> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await userRepository.GetById(request.UserId, cancellationToken);

                if (user is null)
                {
                    throw new NotFoundException(nameof(Application), request.UserId);
                }

                user = request.UserModel.ApplyTo(user);

                return await userRepository.Update(user, cancellationToken);
            }
            catch (NotFoundException) { throw; }
            catch (Exceptions.ValidationException) { throw; }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_UPDATING_USER);
                throw new DataAccessException(ERROR_UPDATING_USER, exception);
            }
        }
    }

}
