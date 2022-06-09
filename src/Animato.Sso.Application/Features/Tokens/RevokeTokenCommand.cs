namespace Animato.Sso.Application.Features.Tokens;
using System;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public class RevokeTokenCommand : IRequest<Unit>
{
    public RevokeTokenCommand(string token) => Token = token;

    public string Token { get; }

    public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, Unit>
    {
        private readonly ITokenRepository tokenRepository;
        private readonly ILogger<RevokeTokenCommandHandler> logger;
        private const string ERROR_UPDATING_TOKEN = "Error updating token";

        public RevokeTokenCommandHandler(ITokenRepository tokenRepository, ILogger<RevokeTokenCommandHandler> logger)
        {
            this.tokenRepository = tokenRepository ?? throw new ArgumentNullException(nameof(tokenRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await tokenRepository.Revoke(request.Token, cancellationToken);
                return Unit.Value;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_UPDATING_TOKEN);
                throw new DataAccessException(ERROR_UPDATING_TOKEN, exception);
            }
        }
    }

    public class RevokeTokenCommandValidator : AbstractValidator<RevokeTokenCommand>
    {
        public RevokeTokenCommandValidator() => RuleFor(v => v.Token).NotEmpty().WithMessage(v => $"{nameof(v.Token)} must have a value");

    }
}
