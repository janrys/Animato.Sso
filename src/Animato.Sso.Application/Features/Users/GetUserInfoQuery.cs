namespace Animato.Sso.Application.Features.Users;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Common.Logging;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Application.Models;
using Animato.Sso.Domain.Entities;
using Animato.Sso.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public class GetUserInfoQuery : IRequest<UserInfoResult>
{
    public GetUserInfoQuery(string accessToken, ClaimsPrincipal user)
    {
        AccessToken = accessToken;
        User = user;
    }

    public string AccessToken { get; }
    public ClaimsPrincipal User { get; }

    public class GetUserInfoQueryValidator : AbstractValidator<GetUserInfoQuery>
    {
        public GetUserInfoQueryValidator() => RuleFor(v => v.AccessToken).NotEmpty().WithMessage(v => $"{nameof(v.AccessToken)} must have a value");
    }

    public class GetUserInfoQueryHandler : IRequestHandler<GetUserInfoQuery, UserInfoResult>
    {
        private readonly IUserRepository userRepository;
        private readonly ITokenRepository tokenRepository;
        private readonly IClaimFactory claimFactory;
        private readonly IDateTimeService dateTimeService;
        private readonly ILogger<GetUserInfoQueryHandler> logger;

        public GetUserInfoQueryHandler(IUserRepository userRepository
            , ITokenRepository tokenRepository
            , IClaimFactory claimFactory
            , IDateTimeService dateTimeService
            , ILogger<GetUserInfoQueryHandler> logger)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.tokenRepository = tokenRepository ?? throw new ArgumentNullException(nameof(tokenRepository));
            this.claimFactory = claimFactory;
            this.dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<UserInfoResult> Handle(GetUserInfoQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var token = await tokenRepository.GetToken(request.AccessToken, cancellationToken);

                if (token is null || token.TokenType != TokenType.Access)
                {
                    throw new NotFoundException("Token not found");
                }

                if (token.IsExpired(dateTimeService.UtcNow))
                {
                    throw new ForbiddenAccessException("Token is expired");
                }

                if (token.Revoked.HasValue)
                {
                    throw new ForbiddenAccessException("Token has been revoked");
                }

                var user = await userRepository.GetById(token.UserId, cancellationToken);

                if (user is null)
                {
                    throw new NotFoundException("User not found");
                }

                var claims = claimFactory.GenerateClaims(user, AuthorizationMethod.Unknown, null);

                var userInfo = new UserInfoResult()
                {
                    Sub = user.Id.Value.ToString(),
                    UserName = user.Login
                };

                claims.ToList().ForEach(c => userInfo.Claims.TryAdd(c.Type, c.Value));

                return userInfo;
            }
            catch (NotFoundException) { throw; }
            catch (Exception exception)
            {
                logger.UsersLoadingError(exception);
                throw new DataAccessException(LogMessageTexts.ErrorLoadingUsers, exception);
            }
        }
    }
}
