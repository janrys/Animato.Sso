namespace Animato.Sso.Application.Features.Users;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Common.Logging;
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

        public GetUsersQueryHandler(IUserRepository userRepository, ILogger<GetUsersQueryHandler> logger)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.logger = logger;
        }

        public async Task<IEnumerable<User>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return await userRepository.GetAll(cancellationToken);
            }
            catch (Exception exception)
            {
                logger.UsersLoadingError(exception);
                throw new DataAccessException("Error loading users", exception);
            }
        }
    }
}
