namespace Animato.Sso.Application.Features.Users;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Application.Models;
using Animato.Sso.Application.Security;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public class AuthorizeUserCommand : IRequest<AuthorizationResult>
{
    public AuthorizeUserCommand(AuthorizationRequest authorizationRequest, ClaimsPrincipal user)
    {
        AuthorizationRequest = authorizationRequest;
        User = user;
    }

    public AuthorizationRequest AuthorizationRequest { get; }
    public ClaimsPrincipal User { get; }

    public class AuthorizeUserCommandHandler : IRequestHandler<AuthorizeUserCommand, AuthorizationResult>
    {
        private readonly IUserRepository userRepository;
        private readonly ILogger<AuthorizeUserCommandHandler> logger;
        private const string ERROR_CREATING_ASSET = "Error creating asset";

        public AuthorizeUserCommandHandler(IUserRepository userRepository, ILogger<AuthorizeUserCommandHandler> logger)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AuthorizationResult> Handle(AuthorizeUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await userRepository.GetUserByUserName(request.User.GetUserName(), cancellationToken);

                if (user == null)
                {
                    throw new NotFoundException(nameof(User), request.User.GetUserName());
                }

                var authorizationResult = new AuthorizationResult();
                return authorizationResult;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_CREATING_ASSET);
                throw new DataAccessException(ERROR_CREATING_ASSET, exception);
            }
        }
    }

    public class AuthorizeUserCommandValidator : AbstractValidator<AuthorizeUserCommand>
    {
        public AuthorizeUserCommandValidator()
        {
            RuleFor(v => v.AuthorizationRequest).NotNull().WithMessage(v => $"{nameof(v.AuthorizationRequest)} must have a value");
            RuleFor(v => v.User).NotEmpty().WithMessage(v => $"{nameof(v.User)} must have a value");
        }

    }
}
