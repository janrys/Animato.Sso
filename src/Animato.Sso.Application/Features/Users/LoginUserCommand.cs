namespace Animato.Sso.Application.Features.Users;
using System;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public class LoginUserCommand : IRequest<User>
{
    public LoginUserCommand(string userName, string password)
    {
        UserName = userName;
        Password = password;
    }

    public string UserName { get; }
    public string Password { get; }

    public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
    {
        public LoginUserCommandValidator()
        {
            RuleFor(v => v.UserName).NotNull().WithMessage(v => $"{nameof(v.UserName)} must have a value");
            RuleFor(v => v.Password).NotEmpty().WithMessage(v => $"{nameof(v.Password)} must have a value");
        }

    }
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, User>
    {
        private readonly IUserRepository userRepository;
        private readonly IPasswordFactory passwordHasher;
        private readonly ILogger<LoginUserCommandHandler> logger;
        private const string ERROR_LOADING_USER = "Error loading user";

        public LoginUserCommandHandler(IUserRepository userRepository, IPasswordFactory passwordHasher, ILogger<LoginUserCommandHandler> logger)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<User> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await userRepository.GetUserByLogin(request.UserName.Trim(), cancellationToken);

                if (user is null
                    || user.IsDeleted
                    || user.IsBlocked
                    || !passwordHasher.IsValid(user.Password, request.Password, user.Salt, user.PasswordHashAlgorithm))
                {
                    return null;
                }

                return user;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_LOADING_USER);
                throw new DataAccessException(ERROR_LOADING_USER, exception);
            }
        }
    }
}
