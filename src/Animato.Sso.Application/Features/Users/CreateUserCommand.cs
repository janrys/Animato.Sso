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

public class CreateUserCommand : IRequest<User>
{
    public CreateUserCommand(CreateUserModel userModel, ClaimsPrincipal user)
    {
        UserModel = userModel;
        User = user;
    }

    public CreateUserModel UserModel { get; }
    public ClaimsPrincipal User { get; }

    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(v => v.UserModel).NotNull().WithMessage(v => $"{nameof(v.UserModel)} must have a value");
            RuleFor(v => v.UserModel.Login).NotEmpty().WithMessage(v => $"{nameof(v.UserModel.Login)} must have a value");
        }
    }

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, User>
    {
        private readonly OidcOptions oidcOptions;
        private readonly IUserRepository userRepository;
        private readonly ITokenFactory tokenFactory;
        private readonly ILogger<CreateUserCommandHandler> logger;
        private readonly IPasswordHasher passwordHasher;
        private const string ERROR_CREATING_USER = "Error creating user";

        public CreateUserCommandHandler(OidcOptions oidcOptions
            , IUserRepository userRepository
            , ITokenFactory tokenFactory
            , IPasswordHasher passwordHasher
            , ILogger<CreateUserCommandHandler> logger)
        {
            this.oidcOptions = oidcOptions ?? throw new ArgumentNullException(nameof(oidcOptions));
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.tokenFactory = tokenFactory ?? throw new ArgumentNullException(nameof(tokenFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        public async Task<User> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await userRepository.GetUserByLogin(request.UserModel.Login, cancellationToken);

                if (user is not null)
                {
                    throw new Exceptions.ValidationException(
                        Exceptions.ValidationException.CreateFailure(nameof(request.UserModel.Login)
                        , $"User with {nameof(request.UserModel.Login)} {request.UserModel.Login} already exists"));
                }

                request.UserModel.ValidateAndSanitize(oidcOptions, tokenFactory);
                user = new User();
                user.UpdatePasswordAndHash(passwordHasher);
                user = request.UserModel.ApplyTo(user);
                return await userRepository.Create(user, cancellationToken);
            }
            catch (Exceptions.ValidationException) { throw; }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_CREATING_USER);
                throw new DataAccessException(ERROR_CREATING_USER, exception);
            }
        }
    }

}
