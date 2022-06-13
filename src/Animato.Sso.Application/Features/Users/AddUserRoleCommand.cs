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

public class AddUserRoleCommand : IRequest<IEnumerable<ApplicationRole>>
{
    public AddUserRoleCommand(UserId userId
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

    public class AddUserRoleCommandValidator : AbstractValidator<AddUserRoleCommand>
    {
        public AddUserRoleCommandValidator()
        {
            RuleFor(v => v.RoleId).NotNull().WithMessage(v => $"{nameof(v.RoleId)} must have a value");
            RuleFor(v => v.UserId).NotNull().WithMessage(v => $"{nameof(v.UserId)} must have a value");
        }
    }

    public class AddUserRoleCommandHandler : IRequestHandler<AddUserRoleCommand, IEnumerable<ApplicationRole>>
    {
        private readonly IUserRepository userRepository;
        private readonly IApplicationRoleRepository roleRepository;
        private readonly ILogger<AddUserRoleCommandHandler> logger;
        private const string ERROR_UPDATING_USER = "Error updating user";

        public AddUserRoleCommandHandler(IUserRepository userRepository
            , IApplicationRoleRepository roleRepository
            , ILogger<AddUserRoleCommandHandler> logger)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<ApplicationRole>> Handle(AddUserRoleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await userRepository.GetById(request.UserId, cancellationToken);

                if (user is null)
                {
                    throw new NotFoundException(nameof(Application), request.UserId);
                }

                var role = await roleRepository.GetById(request.RoleId, cancellationToken);

                if (role is null)
                {
                    throw new NotFoundException(nameof(ApplicationRole), request.RoleId);
                }

                var roles = new List<ApplicationRole>();
                roles.AddRange(await userRepository.GetUserRoles(request.UserId, cancellationToken));

                if (!roles.Any(r => r.Id == request.RoleId))
                {
                    await userRepository.AddUserRole(user.Id, role.Id, cancellationToken);
                }
                roles.Add(role);

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
