namespace Animato.Sso.Application.Features.Users;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Application.Models;
using Animato.Sso.Domain.Entities;
using Animato.Sso.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public class GetTokenCommand : IRequest<TokenResult>
{
    public GetTokenCommand(TokenRequest tokenRequest) => TokenRequest = tokenRequest;

    public TokenRequest TokenRequest { get; }

    public class GetTokenCommandHandler : IRequestHandler<GetTokenCommand, TokenResult>
    {
        private readonly OidcOptions oidcOptions;
        private readonly IUserRepository userRepository;
        private readonly IApplicationRepository applicationRepository;
        private readonly IAuthorizationCodeRepository authorizationCodeRepository;
        private readonly ITokenFactory tokenFactory;
        private readonly ILogger<GetTokenCommandHandler> logger;
        private const string ERROR_CREATING_TOKEN = "Error creating token";

        public GetTokenCommandHandler(OidcOptions oidcOptions, IUserRepository userRepository, IApplicationRepository applicationRepository, IAuthorizationCodeRepository authorizationCodeRepository, ITokenFactory tokenFactory, ILogger<GetTokenCommandHandler> logger)
        {
            this.oidcOptions = oidcOptions ?? throw new ArgumentNullException(nameof(oidcOptions));
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            this.authorizationCodeRepository = authorizationCodeRepository ?? throw new ArgumentNullException(nameof(authorizationCodeRepository));
            this.tokenFactory = tokenFactory ?? throw new ArgumentNullException(nameof(tokenFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TokenResult> Handle(GetTokenCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var code = await authorizationCodeRepository.GetCode(request.TokenRequest.Code, cancellationToken);
                if (code == null)
                {
                    throw new ForbiddenAccessException("", $"Authorization code is not valid. Code can be used just once and prior to expiration");
                }

                if (code.Created < DateTime.UtcNow.AddMinutes(-1 * oidcOptions.CodeExpirationMinutes))
                {
                    throw new ForbiddenAccessException("", $"Authorization code has already expired");
                }

                if (!request.TokenRequest.RedirectUri.Equals(code.RedirectUri, StringComparison.Ordinal))
                {
                    throw new ForbiddenAccessException("", $"Redirect {request.TokenRequest.RedirectUri} is not the same as in authorization request");
                }

                var application = await applicationRepository.GetById(code.ApplicationId, cancellationToken);
                if (application == null || !application.Secrets.Any(s => s.Equals(request.TokenRequest.ClientSecret, StringComparison.Ordinal)))
                {
                    throw new ForbiddenAccessException("", $"Client secret is not valid");
                }

                var user = await userRepository.GetById(code.UserId, cancellationToken);
                if (user == null)
                {
                    throw new ForbiddenAccessException("", $"User {code.UserId} not found");
                }

                var userRoles = await applicationRepository.GetUserRoles(application.Id, code.UserId, cancellationToken);
                if (!userRoles.Any())
                {
                    throw new ForbiddenAccessException("", $"No permission for application {application.Name} ({application.Code})");
                }

                var tokenResult = await GetTokenResult(user, application, userRoles, request.TokenRequest, cancellationToken);
                await authorizationCodeRepository.Delete(code.Code, cancellationToken);
                return tokenResult;
            }
            catch (ForbiddenAccessException) { throw; }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_CREATING_TOKEN);
                throw new DataAccessException(ERROR_CREATING_TOKEN, exception);
            }
        }

        private Task<TokenResult> GetTokenResult(User user, Application application, IEnumerable<ApplicationRole> userRoles, TokenRequest tokenRequest, CancellationToken cancellationToken)
        {
            var grantType = tokenRequest.GrantType.Trim().ToLower(DefaultOptions.Culture);

            if (grantType.Equals(GrantType.Code.GrantCode, StringComparison.OrdinalIgnoreCase))
            {
                return GetTokenFromCode(user, application, userRoles, tokenRequest, cancellationToken);
            }
            else
            {
                throw new Exceptions.ValidationException(
                    Exceptions.ValidationException.CreateFailure(tokenRequest.GrantType
                    , $"Response type value {tokenRequest.GrantType} is invalid. Allowed values are {string.Join(", ", GrantType.GetAll().Select(f => f.GrantCode))}"));
            }
        }

        private Task<TokenResult> GetTokenFromCode(User user, Application application, IEnumerable<ApplicationRole> userRoles, TokenRequest tokenRequest, CancellationToken cancellationToken)
        {
            var tokenResult = new TokenResult()
            {
                IsAuthorized = true,
                GrantType = GrantType.Code,
                AccessToken = tokenFactory.GenerateAccessToken(user, application, userRoles.ToArray())
            };

            var accessToken = new JwtSecurityToken(tokenResult.AccessToken);
            tokenResult.ExpiresIn = (int)accessToken.ValidTo.Subtract(DateTime.UtcNow).TotalSeconds;

            return Task.FromResult(tokenResult);
        }
    }


    public class GetTokenCommandValidator : AbstractValidator<GetTokenCommand>
    {
        public GetTokenCommandValidator()
        {
            RuleFor(v => v.TokenRequest).NotNull().WithMessage(v => $"{nameof(v.TokenRequest)} must have a value");
            RuleFor(v => v.TokenRequest.GrantType).NotNull().WithMessage(v => $"{nameof(v.TokenRequest.GrantType)} must have a value");
        }

    }
}
