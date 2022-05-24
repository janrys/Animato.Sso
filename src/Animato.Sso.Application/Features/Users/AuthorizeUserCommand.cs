namespace Animato.Sso.Application.Features.Users;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Application.Models;
using Animato.Sso.Application.Security;
using Animato.Sso.Domain.Entities;
using Animato.Sso.Domain.Enums;
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
        private readonly IApplicationRepository applicationRepository;
        private readonly IAuthorizationCodeRepository authorizationCodeRepository;
        private readonly ITokenFactory tokenFactory;
        private readonly ILogger<AuthorizeUserCommandHandler> logger;
        private const string ERROR_AUTHORIZING_USER = "Error authorizing user";

        public AuthorizeUserCommandHandler(IUserRepository userRepository, IApplicationRepository applicationRepository, IAuthorizationCodeRepository authorizationCodeRepository, ITokenFactory tokenFactory, ILogger<AuthorizeUserCommandHandler> logger)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            this.authorizationCodeRepository = authorizationCodeRepository ?? throw new ArgumentNullException(nameof(authorizationCodeRepository));
            this.tokenFactory = tokenFactory ?? throw new ArgumentNullException(nameof(tokenFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AuthorizationResult> Handle(AuthorizeUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await userRepository.GetUserByUserName(request.User.GetUserName(), cancellationToken);
                if (user == null)
                {
                    throw new ForbiddenAccessException(user.Login, $"User {request.User.GetUserName()} not found");
                }

                var application = await applicationRepository.GetByCode(request.AuthorizationRequest.ClientId, cancellationToken);
                if (application == null)
                {
                    throw new ForbiddenAccessException(user.Login, $"Client {request.AuthorizationRequest.ClientId} not found");
                }

                if (!IsRedirectUriValid(application, request.AuthorizationRequest.RedirectUri))
                {
                    throw new ForbiddenAccessException(user.Login, $"Redirect {request.AuthorizationRequest.RedirectUri} is not allowed for application {application.Name} ({application.Code})");
                }

                var userRoles = await applicationRepository.GetUserRoles(application.Id, user.Id, cancellationToken);
                if (!userRoles.Any())
                {
                    throw new ForbiddenAccessException(user.Login, $"No permission for application {application.Name} ({application.Code})");
                }

                var authorizationResult = await AuthorizeUser(user, application, userRoles, request.AuthorizationRequest, cancellationToken);
                return authorizationResult;
            }
            catch (ForbiddenAccessException) { throw; }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_AUTHORIZING_USER);
                throw new DataAccessException(ERROR_AUTHORIZING_USER, exception);
            }
        }

        private bool IsRedirectUriValid(Application application, string redirectUri)
        {
            if (string.IsNullOrEmpty(redirectUri)
                || application.RedirectUris is null
                || !application.RedirectUris.Any())
            {
                return false;
            }

            return application.RedirectUris.Any(r => redirectUri.StartsWith(r, StringComparison.InvariantCultureIgnoreCase));
        }

        private Task<AuthorizationResult> AuthorizeUser(User user, Application application, IEnumerable<ApplicationRole> userRoles, AuthorizationRequest authorizationRequest, CancellationToken cancellationToken)
        {
            var responseType = authorizationRequest.ResponseType.Trim().ToLower(DefaultOptions.Culture);

            if (responseType.Equals(AuthorizationFlowType.Code.RequestCode, StringComparison.OrdinalIgnoreCase))
            {
                return AuthorizeUserCodeFlow(user, application, userRoles, authorizationRequest, cancellationToken);
            }
            else if (responseType.Equals(AuthorizationFlowType.Token.RequestCode, StringComparison.OrdinalIgnoreCase))
            {
                return AuthorizeUserImplicitFlow(user, application, userRoles, authorizationRequest, cancellationToken);
            }
            else
            {
                throw new Exceptions.ValidationException(
                    Exceptions.ValidationException.CreateFailure(authorizationRequest.ResponseType
                    , $"Response type value {authorizationRequest.ResponseType} is invalid. Allowed values are {string.Join(", ", AuthorizationFlowType.GetAll().Select(f => f.RequestCode))}"));
            }
        }

        private Task<AuthorizationResult> AuthorizeUserImplicitFlow(User user, Application application, IEnumerable<ApplicationRole> userRoles, AuthorizationRequest authorizationRequest, CancellationToken cancellationToken)
        {
            var authorizationResult = new AuthorizationResult
            {
                IsAuthorized = true,
                AuthorizationFlow = AuthorizationFlowType.Token,
                AccessToken = tokenFactory.GenerateAccessToken(user, application, userRoles.ToArray())
            };

            return Task.FromResult(authorizationResult);
        }

        private async Task<AuthorizationResult> AuthorizeUserCodeFlow(User user, Application application, IEnumerable<ApplicationRole> userRoles, AuthorizationRequest authorizationRequest, CancellationToken cancellationToken)
        {
            var authorizationResult = new AuthorizationResult
            {
                IsAuthorized = true,
                AuthorizationFlow = AuthorizationFlowType.Code,
                Code = tokenFactory.GenerateCode()
            };

            var authorizationCode = new AuthorizationCode()
            {
                Code = authorizationResult.Code,
                ApplicationId = application.Id,
                UserId = user.Id,
                ClientId = authorizationRequest.ClientId,
                RedirectUri = authorizationRequest.RedirectUri,
                Scope = authorizationRequest.Scope,
                Created = DateTime.UtcNow
            };

            await authorizationCodeRepository.Insert(authorizationCode, cancellationToken);
            return authorizationResult;
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
