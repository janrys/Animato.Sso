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

public class RemoveUserRoleCommand : IRequest<IEnumerable<ApplicationRole>>
{
    public RemoveUserRoleCommand(UserId userId
        , ApplicationRoleId roleId
        , ClaimsPrincipal user)
    {
        UserId = userId;
        RoleId = roleId;
        User = user;
    }

    public UserId UserId { get; }
    public ApplicationRoleId RoleId { get; }
    public ClaimsPrincipal User { get; }

    public class RemoveUserRoleCommandValidator : AbstractValidator<RemoveUserRoleCommand>
    {
        public RemoveUserRoleCommandValidator()
        {
            RuleFor(v => v.RoleId).NotNull().WithMessage(v => $"{nameof(v.RoleId)} must have a value");
            RuleFor(v => v.UserId).NotNull().WithMessage(v => $"{nameof(v.UserId)} must have a value");
        }
    }

    public class RemoveUserRoleCommandHandler : IRequestHandler<RemoveUserRoleCommand, IEnumerable<ApplicationRole>>
    {
        private readonly IUserRepository userRepository;
        private readonly IApplicationRoleRepository applicationRoleRepository;
        private readonly ILogger<RemoveUserRoleCommandHandler> logger;
        private const string ERROR_UPDATING_USER = "Error updating user";

        public RemoveUserRoleCommandHandler(IUserRepository userRepository
            , IApplicationRoleRepository applicationRoleRepository
            , ILogger<RemoveUserRoleCommandHandler> logger)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.applicationRoleRepository = applicationRoleRepository ?? throw new ArgumentNullException(nameof(applicationRoleRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<ApplicationRole>> Handle(RemoveUserRoleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await userRepository.GetById(request.UserId, cancellationToken);

                if (user is null)
                {
                    throw new NotFoundException(nameof(User), request.UserId);
                }

                var roles = new List<ApplicationRole>();
                roles.AddRange(await applicationRoleRepository.GetByUser(request.UserId, cancellationToken));

                if (roles.Any(r => r.Id == request.RoleId))
                {
                    await userRepository.RemoveUserRole(user.Id, request.RoleId, cancellationToken);
                }
                roles.RemoveAll(r => r.Id == request.RoleId);

                return roles;
            }
            catch (NotFoundException) { throw; }
            catch (Exceptions.ValidationException) { throw; }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_UPDATING_USER);
                throw new DataAccessException(ERROR_UPDATING_USER, exception);
            }
        }
    }

}
