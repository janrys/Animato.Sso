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

public class GetUserByUserNameQuery : IRequest<User>
{
    public GetUserByUserNameQuery(string userName, ClaimsPrincipal user)
    {
        UserName = userName;
        User = user;
    }

    public string UserName { get; }
    public ClaimsPrincipal User { get; }

    public class GetUserByUserNameQueryValidator : AbstractValidator<GetUserByUserNameQuery>
    {
        public GetUserByUserNameQueryValidator()
            => RuleFor(v => v.UserName).NotEmpty().WithMessage(v => $"{nameof(v.UserName)} must have a value");

    }

    public class GetUserByUserNameQueryHandler : IRequestHandler<GetUserByUserNameQuery, User>
    {
        private readonly IUserRepository userRepository;
        private readonly ILogger<GetUserByUserNameQueryHandler> logger;

        public GetUserByUserNameQueryHandler(IUserRepository userRepository, ILogger<GetUserByUserNameQueryHandler> logger)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.logger = logger;
        }

        public async Task<User> Handle(GetUserByUserNameQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return await userRepository.GetUserByLogin(request.UserName, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.UsersLoadingError(exception);
                throw new DataAccessException(LogMessageTexts.ErrorLoadingUsers, exception);
            }
        }
    }
}
