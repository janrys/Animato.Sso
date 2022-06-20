namespace Animato.Sso.Application.Features.Users;
using System;
using System.Collections.Generic;
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
        private readonly ITokenRepository tokenRepository;
        private readonly IDateTimeService dateTime;
        private readonly ITokenFactory tokenFactory;
        private readonly ILogger<GetTokenCommandHandler> logger;
        private const string ERROR_CREATING_TOKEN = "Error creating token";

        public GetTokenCommandHandler(OidcOptions oidcOptions
            , IUserRepository userRepository
            , IApplicationRepository applicationRepository
            , IAuthorizationCodeRepository authorizationCodeRepository
            , ITokenRepository tokenRepository
            , IDateTimeService dateTime
            , ITokenFactory tokenFactory, ILogger<GetTokenCommandHandler> logger)
        {
            this.oidcOptions = oidcOptions ?? throw new ArgumentNullException(nameof(oidcOptions));
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            this.authorizationCodeRepository = authorizationCodeRepository ?? throw new ArgumentNullException(nameof(authorizationCodeRepository));
            this.tokenRepository = tokenRepository ?? throw new ArgumentNullException(nameof(tokenRepository));
            this.dateTime = dateTime ?? throw new ArgumentNullException(nameof(dateTime));
            this.tokenFactory = tokenFactory ?? throw new ArgumentNullException(nameof(tokenFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TokenResult> Handle(GetTokenCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var grantType = GrantType.GetAll().FirstOrDefault(g => g.GrantCode
                    .Equals(request.TokenRequest.GrantType.Trim().ToLower(GlobalOptions.Culture),
                    StringComparison.OrdinalIgnoreCase));

                if (grantType is null)
                {
                    throw new Exceptions.ValidationException(
                        Exceptions.ValidationException.CreateFailure(request.TokenRequest.GrantType
                        , $"Response type value {request.TokenRequest.GrantType} is invalid. Allowed values are {string.Join(", ", GrantType.GetAll().Select(f => f.GrantCode))}"));
                }

                AuthorizationCode code;
                Token refreshToken = null;
                Domain.Entities.ApplicationId applicationId;
                UserId userId;

                if (grantType == GrantType.Code)
                {
                    code = await authorizationCodeRepository.GetCode(request.TokenRequest.Code, cancellationToken);
                    if (code == null)
                    {
                        throw new ForbiddenAccessException("", $"Authorization code is not valid. Code can be used just once and prior to expiration");
                    }

                    if (code.Created < dateTime.UtcNow.AddMinutes(-1 * oidcOptions.CodeExpirationMinutes))
                    {
                        throw new ForbiddenAccessException("", $"Authorization code has already expired");
                    }

                    if (!request.TokenRequest.RedirectUri.Equals(code.RedirectUri, StringComparison.Ordinal))
                    {
                        throw new ForbiddenAccessException("", $"Redirect {request.TokenRequest.RedirectUri} is not the same as in authorization request");
                    }

                    await authorizationCodeRepository.Delete(code.Code, cancellationToken);
                    userId = code.UserId;
                    applicationId = code.ApplicationId;
                }
                else if (grantType == GrantType.Refresh)
                {
                    refreshToken = await tokenRepository.GetToken(request.TokenRequest.RefreshToken, cancellationToken);

                    if (refreshToken == null)
                    {
                        throw new ForbiddenAccessException("", $"Refresh token is not valid");
                    }

                    if (refreshToken.Revoked.HasValue)
                    {
                        throw new ForbiddenAccessException("", $"Refresh token has been revoked");
                    }

                    if (refreshToken.IsExpired(dateTime.UtcNow))
                    {
                        throw new ForbiddenAccessException("", $"Refresh token has expired");
                    }

                    userId = refreshToken.UserId;
                    applicationId = refreshToken.ApplicationId;
                }
                else
                {
                    throw new Exceptions.ValidationException(
                        Exceptions.ValidationException.CreateFailure(grantType.GrantCode
                        , $"Response type value {grantType.GrantCode} is invalid. Allowed values are {string.Join(", ", GrantType.GetAll().Select(f => f.GrantCode))}"));
                }

                var application = await applicationRepository.GetById(applicationId, cancellationToken);
                if (application == null || !application.Secrets.Any(s => s.Equals(request.TokenRequest.ClientSecret, StringComparison.Ordinal)))
                {
                    throw new ForbiddenAccessException("", $"Client secret is not valid");
                }

                var user = await userRepository.GetById(userId, cancellationToken);
                if (user == null || user.IsDeleted)
                {
                    throw new ForbiddenAccessException("", $"User {userId} not found");
                }

                if (user.IsBlocked)
                {
                    throw new ForbiddenAccessException("", $"User {userId} is blocked");
                }

                var userRoles = await applicationRepository.GetUserRoles(application.Id, userId, cancellationToken);
                if (!userRoles.Any())
                {
                    throw new ForbiddenAccessException("", $"No permission for application {application.Name} ({application.Code})");
                }

                TokenResult tokenResult;
                if (grantType == GrantType.Code)
                {
                    tokenResult = await GetTokenFromCode(user, application, userRoles, request.TokenRequest, cancellationToken);
                }
                else if (grantType == GrantType.Refresh)
                {
                    tokenResult = await GetTokenFromRefreshToken(refreshToken, user, application, userRoles, request.TokenRequest, cancellationToken);
                }
                else
                {
                    throw new Exceptions.ValidationException(
                        Exceptions.ValidationException.CreateFailure(grantType.GrantCode
                        , $"Response type value {grantType.GrantCode} is invalid. Allowed values are {string.Join(", ", GrantType.GetAll().Select(f => f.GrantCode))}"));
                }

                return tokenResult;
            }
            catch (Exceptions.ValidationException) { throw; }
            catch (ForbiddenAccessException) { throw; }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_CREATING_TOKEN);
                throw new DataAccessException(ERROR_CREATING_TOKEN, exception);
            }
        }

        private async Task<TokenResult> GetTokenFromRefreshToken(Token refreshToken, User user, Application application, IEnumerable<ApplicationRole> userRoles, TokenRequest tokenRequest, CancellationToken cancellationToken)
        {
            var accessToken = new Token()
            {
                Value = tokenFactory.GenerateAccessToken(user, application, userRoles.ToArray()),
                ApplicationId = application.Id,
                Created = dateTime.UtcNow,
                Expiration = dateTime.UtcNow.AddMinutes(application.AccessTokenExpirationMinutes),
                TokenType = TokenType.Access,
                UserId = user.Id,
                RefreshTokenId = refreshToken.RefreshTokenId,
            };

            var tokenResult = new TokenResult()
            {
                IsAuthorized = true,
                GrantType = GrantType.Refresh,
                AccessToken = accessToken.Value
            };

            await tokenRepository.Create(accessToken, cancellationToken);

            tokenResult.ExpiresIn = (int)accessToken.Expiration.Subtract(dateTime.UtcNow).TotalSeconds;
            return tokenResult;
        }

        private async Task<TokenResult> GetTokenFromCode(User user, Application application, IEnumerable<ApplicationRole> userRoles, TokenRequest tokenRequest, CancellationToken cancellationToken)
        {
            var accessToken = new Token()
            {
                Value = tokenFactory.GenerateAccessToken(user, application, userRoles.ToArray()),
                ApplicationId = application.Id,
                Created = dateTime.UtcNow,
                Expiration = dateTime.UtcNow.AddMinutes(application.AccessTokenExpirationMinutes),
                TokenType = TokenType.Access,
                UserId = user.Id
            };

            var refreshToken = new Token()
            {
                Value = tokenFactory.GenerateRefreshToken(user),
                ApplicationId = application.Id,
                Created = dateTime.UtcNow,
                Expiration = dateTime.UtcNow.AddMinutes(application.RefreshTokenExpirationMinutes),
                TokenType = TokenType.Refresh,
                UserId = user.Id
            };

            var idToken = new Token()
            {
                Value = tokenFactory.GenerateIdToken(user, application, userRoles.ToArray()),
                ApplicationId = application.Id,
                Created = dateTime.UtcNow,
                Expiration = dateTime.UtcNow.AddMinutes(application.RefreshTokenExpirationMinutes),
                TokenType = TokenType.Id,
                UserId = user.Id
            };

            var tokenResult = new TokenResult()
            {
                IsAuthorized = true,
                GrantType = GrantType.Code,
                AccessToken = accessToken.Value,
                RefreshToken = refreshToken.Value,
                IdToken = idToken.Value,
            };

            await tokenRepository.Create(accessToken, refreshToken, idToken, cancellationToken);

            tokenResult.ExpiresIn = (int)accessToken.Expiration.Subtract(dateTime.UtcNow).TotalSeconds;
            tokenResult.RefreshTokenExpiresIn = (int)refreshToken.Expiration.Subtract(dateTime.UtcNow).TotalSeconds;

            return tokenResult;
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
