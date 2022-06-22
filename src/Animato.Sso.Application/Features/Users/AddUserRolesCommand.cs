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

public class AddUserRolesCommand : IRequest<IEnumerable<ApplicationRole>>
{
    public AddUserRolesCommand(UserId userId
        , ClaimsPrincipal user
        , params ApplicationRoleId[] roleIds
        )
    {
        UserId = userId;
        RoleIds = roleIds;
        User = user;
    }

    public UserId UserId { get; }
    public ApplicationRoleId[] RoleIds { get; }
    public ClaimsPrincipal User { get; }

    public class AddUserRoleCommandValidator : AbstractValidator<AddUserRolesCommand>
    {
        public AddUserRoleCommandValidator()
        {
            RuleFor(v => v.RoleIds).NotEmpty().WithMessage(v => $"{nameof(v.RoleIds)} must have a value");
            RuleFor(v => v.UserId).NotNull().WithMessage(v => $"{nameof(v.UserId)} must have a value");
        }
    }

    public class AddUserRoleCommandHandler : IRequestHandler<AddUserRolesCommand, IEnumerable<ApplicationRole>>
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

        public async Task<IEnumerable<ApplicationRole>> Handle(AddUserRolesCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await userRepository.GetById(request.UserId, cancellationToken);

                if (user is null)
                {
                    throw new NotFoundException(nameof(Application), request.UserId);
                }

                var applicationRoles = await roleRepository.GetByIds(cancellationToken, request.RoleIds);
                var missingApplicationRoleIds = request.RoleIds.Where(r => !applicationRoles.Any(a => a.Id == r));

                if (missingApplicationRoleIds.Any())
                {
                    throw new NotFoundException(nameof(ApplicationRole), string.Join(", ", missingApplicationRoleIds.Select(r => r.Value)));
                }

                var userRoles = await userRepository.GetUserRoles(request.UserId, cancellationToken);
                applicationRoles = applicationRoles.Where(a => !userRoles.Any(r => r.Id == a.Id));

                if (applicationRoles.Any())
                {
                    await userRepository.AddUserRoles(user.Id, cancellationToken, applicationRoles.Select(r => r.Id).ToArray());
                }

                return applicationRoles;
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
