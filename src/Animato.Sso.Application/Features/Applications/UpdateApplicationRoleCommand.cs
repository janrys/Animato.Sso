namespace Animato.Sso.Application.Features.Applications;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Application.Features.Applications.DTOs;
using Animato.Sso.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public class UpdateApplicationRoleCommand : IRequest<ApplicationRole>
{
    public UpdateApplicationRoleCommand(ApplicationRoleId roleId
        , CreateApplicationRoleModel role
        , ClaimsPrincipal user)
    {
        RoleId = roleId;
        Role = role;
        User = user;
    }

    public ApplicationRoleId RoleId { get; }
    public CreateApplicationRoleModel Role { get; }
    public ClaimsPrincipal User { get; }

    public class UpdateApplicationRoleCommandValidator : AbstractValidator<UpdateApplicationRoleCommand>
    {
        public UpdateApplicationRoleCommandValidator()
        {
            RuleFor(v => v.RoleId).NotNull().WithMessage(v => $"{nameof(v.RoleId)} must have a value");
            RuleFor(v => v.Role).NotEmpty().WithMessage(v => $"{nameof(v.Role)} must have a value");
            RuleFor(v => v.Role.Name).NotEmpty().WithMessage(v => $"{nameof(v.Role.Name)} must have a value");
        }
    }

    public class UpdateApplicationRoleCommandHandler : IRequestHandler<UpdateApplicationRoleCommand, ApplicationRole>
    {
        private readonly OidcOptions oidcOptions;
        private readonly IApplicationRoleRepository roleRepository;
        private readonly ITokenFactory tokenFactory;
        private readonly ILogger<UpdateApplicationRoleCommandHandler> logger;
        private const string ERROR_UPDATING_APPLICATION_ROLE = "Error updating application role";

        public UpdateApplicationRoleCommandHandler(OidcOptions oidcOptions
            , IApplicationRoleRepository roleRepository
            , ITokenFactory tokenFactory
            , ILogger<UpdateApplicationRoleCommandHandler> logger)
        {
            this.oidcOptions = oidcOptions ?? throw new ArgumentNullException(nameof(oidcOptions));
            this.roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            this.tokenFactory = tokenFactory ?? throw new ArgumentNullException(nameof(tokenFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApplicationRole> Handle(UpdateApplicationRoleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var role = await roleRepository.GetById(request.RoleId, cancellationToken);

                if (role is null)
                {
                    throw new NotFoundException(nameof(ApplicationRole), request.RoleId);
                }

                request.Role.ValidateAndSanitize();
                role = request.Role.ApplyTo(role);

                return await roleRepository.Update(role, cancellationToken);
            }
            catch (NotFoundException) { throw; }
            catch (Exceptions.ValidationException) { throw; }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_UPDATING_APPLICATION_ROLE);
                throw new DataAccessException(ERROR_UPDATING_APPLICATION_ROLE, exception);
            }
        }
    }

}
