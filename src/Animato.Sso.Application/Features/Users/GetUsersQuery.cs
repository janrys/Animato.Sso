namespace Animato.Sso.Application.Features.Users;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

public class GetUsersQuery : IRequest<IEnumerable<User>>
{
    public GetUsersQuery(ClaimsPrincipal user) => User = user;

    public ClaimsPrincipal User { get; }

    public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, IEnumerable<User>>
    {
        private readonly IUserRepository userRepository;
        private readonly ILogger<GetUsersQueryHandler> logger;
        private const string ERROR_LOADING_USERS = "Error loading users";

        public GetUsersQueryHandler(IUserRepository userRepository, ILogger<GetUsersQueryHandler> logger)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.logger = logger;
        }

        public async Task<IEnumerable<User>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return await userRepository.GetUsers(cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_LOADING_USERS);
                throw new DataAccessException(ERROR_LOADING_USERS, exception);
            }
        }
    }
}
