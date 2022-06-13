namespace Animato.Sso.Application.Features.Tokens;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Application.Security;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public class RevokeAllTokensCommand : IRequest<Unit>
{
    public RevokeAllTokensCommand(ClaimsPrincipal user) => User = user;

    public ClaimsPrincipal User { get; }

    public class RevokeAllTokensCommandHandler : IRequestHandler<RevokeAllTokensCommand, Unit>
    {
        private readonly IUserRepository userRepository;
        private readonly ITokenRepository tokenRepository;
        private readonly ILogger<RevokeAllTokensCommandHandler> logger;
        private const string ERROR_UPDATING_TOKEN = "Error updating token";

        public RevokeAllTokensCommandHandler(IUserRepository userRepository
            , ITokenRepository tokenRepository
            , ILogger<RevokeAllTokensCommandHandler> logger)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.tokenRepository = tokenRepository ?? throw new ArgumentNullException(nameof(tokenRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(RevokeAllTokensCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await userRepository.GetUserByLogin(request.User.GetUserName(), cancellationToken);
                if (user == null)
                {
                    throw new ForbiddenAccessException(user.Login, $"User {request.User.GetUserName()} not found");
                }

                await tokenRepository.RevokeTokensForUser(user.Id, cancellationToken);

                return Unit.Value;
            }
            catch (ForbiddenAccessException) { throw; }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_UPDATING_TOKEN);
                throw new DataAccessException(ERROR_UPDATING_TOKEN, exception);
            }
        }
    }

    public class RevokeAllTokensCommandValidator : AbstractValidator<RevokeAllTokensCommand>
    {
        public RevokeAllTokensCommandValidator() => RuleFor(v => v.User).NotNull().WithMessage(v => $"{nameof(v.User)} must have a value");

    }
}
