namespace Animato.Sso.Application.Features.Applications;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public class DeleteApplicationRoleCommand : IRequest<Unit>
{
    public DeleteApplicationRoleCommand(Domain.Entities.ApplicationRoleId roleId, ClaimsPrincipal user)
    {
        RoleId = roleId;
        User = user;
    }

    public Domain.Entities.ApplicationRoleId RoleId { get; }
    public ClaimsPrincipal User { get; }

    public class DeleteApplicationRoleCommandValidator : AbstractValidator<DeleteApplicationRoleCommand>
    {
        public DeleteApplicationRoleCommandValidator()
            => RuleFor(v => v.RoleId).NotNull().WithMessage(v => $"{nameof(v.RoleId)} must have a value");
    }

    public class DeleteApplicationRoleCommandHandler : IRequestHandler<DeleteApplicationRoleCommand, Unit>
    {
        private readonly IApplicationRoleRepository roleRepository;
        private readonly IUserRepository userRepository;
        private readonly ILogger<DeleteApplicationRoleCommandHandler> logger;
        private const string ERROR_DELETING_APPLICATION_ROLES = "Error deleting application roles";

        public DeleteApplicationRoleCommandHandler(IApplicationRoleRepository roleRepository
            , IUserRepository userRepository
            , ILogger<DeleteApplicationRoleCommandHandler> logger)
        {
            this.roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(DeleteApplicationRoleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var role = await roleRepository.GetById(request.RoleId, cancellationToken);

                if (role is null)
                {
                    return Unit.Value;
                }

                var users = await userRepository.GetUserByRole(request.RoleId);
                if (users != null && users.Any())
                {
                    throw new Exceptions.ValidationException(
                        Exceptions.ValidationException.CreateFailure("UserRole", $"Role has {users.Count()} users")
                        );
                }

                await roleRepository.Delete(request.RoleId, cancellationToken);
                return Unit.Value;
            }
            catch (Exceptions.ValidationException) { throw; }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_DELETING_APPLICATION_ROLES);
                throw new DataAccessException(ERROR_DELETING_APPLICATION_ROLES, exception);
            }
        }
    }

}
