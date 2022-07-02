namespace Animato.Sso.Application.Features.Users;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Common.Logging;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public class GetUserClaimsQuery : IRequest<IEnumerable<UserClaim>>
{
    public GetUserClaimsQuery(UserId id, ClaimsPrincipal user)
    {
        Id = id;
        User = user;
    }

    public UserId Id { get; }
    public ClaimsPrincipal User { get; }

    public class GetUserClaimsQueryValidator : AbstractValidator<GetUserClaimsQuery>
    {
        public GetUserClaimsQueryValidator() => RuleFor(v => v.Id).NotNull().WithMessage(v => $"{nameof(v.Id)} must have a value");
    }

    public class GetUserClaimsQueryHandler : IRequestHandler<GetUserClaimsQuery, IEnumerable<UserClaim>>
    {
        private readonly IUserRepository userRepository;
        private readonly ILogger<GetUserClaimsQueryHandler> logger;

        public GetUserClaimsQueryHandler(IUserRepository userRepository, ILogger<GetUserClaimsQueryHandler> logger)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.logger = logger;
        }

        public async Task<IEnumerable<UserClaim>> Handle(GetUserClaimsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return await userRepository.GetClaims(request.Id, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.UsersLoadingError(exception);
                throw new DataAccessException(LogMessageTexts.ErrorLoadingUsers, exception);
            }
        }
    }
}
