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

public class GetUserByIdQuery : IRequest<User>
{
    public GetUserByIdQuery(UserId id, ClaimsPrincipal user)
    {
        Id = id;
        User = user;
    }

    public UserId Id { get; }
    public ClaimsPrincipal User { get; }

    public class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
    {
        public GetUserByIdQueryValidator() => RuleFor(v => v.Id).NotNull().WithMessage(v => $"{nameof(v.Id)} must have a value");
    }

    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, User>
    {
        private readonly IUserRepository userRepository;
        private readonly ILogger<GetUserByIdQueryHandler> logger;
        private const string ERROR_LOADING_USERS = "Error loading users";

        public GetUserByIdQueryHandler(IUserRepository userRepository, ILogger<GetUserByIdQueryHandler> logger)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.logger = logger;
        }

        public async Task<User> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return await userRepository.GetById(request.Id, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_LOADING_USERS);
                throw new DataAccessException(ERROR_LOADING_USERS, exception);
            }
        }
    }
}
