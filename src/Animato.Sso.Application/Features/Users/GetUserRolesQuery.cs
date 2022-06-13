namespace Animato.Sso.Application.Features.Users;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public class GetUserRolesQuery : IRequest<IEnumerable<ApplicationRole>>
{
    public GetUserRolesQuery(UserId id, ClaimsPrincipal user)
    {
        Id = id;
        User = user;
    }

    public UserId Id { get; }
    public ClaimsPrincipal User { get; }

    public class GetUserRolesQueryValidator : AbstractValidator<GetUserRolesQuery>
    {
        public GetUserRolesQueryValidator() => RuleFor(v => v.Id).NotNull().WithMessage(v => $"{nameof(v.Id)} must have a value");
    }

    public class GetUserRolesQueryHandler : IRequestHandler<GetUserRolesQuery, IEnumerable<ApplicationRole>>
    {
        private readonly IUserRepository userRepository;
        private readonly ILogger<GetUserRolesQueryHandler> logger;
        private const string ERROR_LOADING_USERS = "Error loading users";

        public GetUserRolesQueryHandler(IUserRepository userRepository, ILogger<GetUserRolesQueryHandler> logger)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.logger = logger;
        }

        public async Task<IEnumerable<ApplicationRole>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return await userRepository.GetUserRoles(request.Id, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_LOADING_USERS);
                throw new DataAccessException(ERROR_LOADING_USERS, exception);
            }
        }
    }
}
