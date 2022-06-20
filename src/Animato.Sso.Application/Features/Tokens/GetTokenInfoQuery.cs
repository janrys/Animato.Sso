namespace Animato.Sso.Application.Features.Tokens;
using System;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Application.Models;
using Animato.Sso.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public class GetTokenInfoQuery : IRequest<TokenInfo>
{
    public GetTokenInfoQuery(string token) => Token = token;

    public string Token { get; }

    public class GetTokenInfoQueryHandler : IRequestHandler<GetTokenInfoQuery, TokenInfo>
    {
        private readonly ITokenRepository tokenRepository;
        private readonly IUserRepository userRepository;
        private readonly IApplicationRepository applicationRepository;
        private readonly IMetadataService metadataService;
        private readonly ITokenFactory tokenFactory;
        private readonly IDateTimeService dateTime;
        private readonly ILogger<GetTokenInfoQueryHandler> logger;
        private const string ERROR_LOADING_TOKEN = "Error loading token";

        public GetTokenInfoQueryHandler(ITokenRepository tokenRepository
            , IUserRepository userRepository
            , IApplicationRepository applicationRepository
            , IMetadataService metadataService
            , ITokenFactory tokenFactory
            , IDateTimeService dateTime
            , ILogger<GetTokenInfoQueryHandler> logger)
        {
            this.tokenRepository = tokenRepository ?? throw new ArgumentNullException(nameof(tokenRepository));
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            this.metadataService = metadataService ?? throw new ArgumentNullException(nameof(metadataService));
            this.tokenFactory = tokenFactory ?? throw new ArgumentNullException(nameof(tokenFactory));
            this.dateTime = dateTime ?? throw new ArgumentNullException(nameof(dateTime));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TokenInfo> Handle(GetTokenInfoQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var token = await tokenRepository.GetToken(request.Token, cancellationToken);
                TokenInfo tokenInfo;

                if (token is null || token.Revoked.HasValue || token.IsExpired(dateTime.UtcNow))
                {
                    return new TokenInfo() { Active = false };
                }

                if (token.TokenType == Domain.Enums.TokenType.Access)
                {
                    tokenInfo = tokenFactory.GetTokenInfo(token.Value);
                    tokenInfo.Active = true;
                }
                else if (token.TokenType == Domain.Enums.TokenType.Refresh)
                {
                    tokenInfo = new TokenInfo() { Active = true };

                    var user = await userRepository.GetById(token.UserId, cancellationToken);
                    var application = await applicationRepository.GetById(token.ApplicationId, cancellationToken);

                    tokenInfo.Audience = application.Code;
                    tokenInfo.ClientId = application.Code;
                    tokenInfo.Issuer = metadataService.GetIssuer();
                    tokenInfo.UserName = user.Login;
                    tokenInfo.IssuedAt = ((DateTimeOffset)token.Created).ToUnixTimeSeconds();
                    tokenInfo.Expiration = ((DateTimeOffset)token.Expiration).ToUnixTimeSeconds();
                }
                else
                {
                    tokenInfo = new TokenInfo() { Active = true };
                }

                return tokenInfo;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_LOADING_TOKEN);
                throw new DataAccessException(ERROR_LOADING_TOKEN, exception);
            }
        }
    }

    public class GetTokenInfoQueryValidator : AbstractValidator<GetTokenInfoQuery>
    {
        public GetTokenInfoQueryValidator() => RuleFor(v => v.Token).NotEmpty().WithMessage(v => $"{nameof(v.Token)} must have a value");

    }
}
