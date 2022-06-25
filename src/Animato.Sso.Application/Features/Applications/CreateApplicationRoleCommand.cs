namespace Animato.Sso.Application.Features.Applications;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Common.Logging;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Application.Features.Applications.DTOs;
using Animato.Sso.Application.Features.Users.DTOs;
using Animato.Sso.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public class CreateApplicationRoleCommand : IRequest<IEnumerable<ApplicationRole>>
{
    public CreateApplicationRoleCommand(Domain.Entities.ApplicationId applicationId, CreateApplicationRolesModel roles, ClaimsPrincipal user)
    {
        ApplicationId = applicationId;
        Roles = roles;
        User = user;
    }

    public Domain.Entities.ApplicationId ApplicationId { get; }
    public CreateApplicationRolesModel Roles { get; }
    public ClaimsPrincipal User { get; }

    public class CreateApplicationRoleCommandValidator : AbstractValidator<CreateApplicationRoleCommand>
    {
        public CreateApplicationRoleCommandValidator()
        {
            RuleFor(v => v.ApplicationId).NotNull().WithMessage(v => $"{nameof(v.ApplicationId)} must have a value");
            RuleFor(v => v.Roles).NotNull().WithMessage(v => $"{nameof(v.Roles)} must have a value");
            RuleFor(v => v.Roles.Names).NotEmpty().WithMessage(v => $"{nameof(v.Roles.Names)} must have a value");
        }
    }

    public class CreateApplicationRoleCommandHandler : IRequestHandler<CreateApplicationRoleCommand, IEnumerable<ApplicationRole>>
    {
        private readonly OidcOptions oidcOptions;
        private readonly IApplicationRepository applicationRepository;
        private readonly IApplicationRoleRepository roleRepository;
        private readonly ITokenFactory tokenFactory;
        private readonly ILogger<CreateApplicationRoleCommandHandler> logger;

        public CreateApplicationRoleCommandHandler(OidcOptions oidcOptions
            , IApplicationRepository applicationRepository
            , IApplicationRoleRepository roleRepository
            , ITokenFactory tokenFactory
            , ILogger<CreateApplicationRoleCommandHandler> logger)
        {
            this.oidcOptions = oidcOptions ?? throw new ArgumentNullException(nameof(oidcOptions));
            this.applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            this.roleRepository = roleRepository;
            this.tokenFactory = tokenFactory ?? throw new ArgumentNullException(nameof(tokenFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<ApplicationRole>> Handle(CreateApplicationRoleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var application = await applicationRepository.GetById(request.ApplicationId, cancellationToken);

                if (application is null)
                {
                    throw new NotFoundException(nameof(Application)
                        , $"Application with id {request.ApplicationId} does not exist");
                }

                var roles = new List<ApplicationRole>();

                foreach (var roleName in request.Roles.Names)
                {
                    var createRoleModel = new CreateApplicationRoleModel() { Name = roleName };
                    createRoleModel.ValidateAndSanitize();
                    var role = new ApplicationRole();
                    role = createRoleModel.ApplyTo(role);
                    role.ApplicationId = request.ApplicationId;
                    roles.Add(role);
                }

                return await roleRepository.Create(cancellationToken, roles.ToArray());
            }
            catch (NotFoundException) { throw; }
            catch (Exceptions.ValidationException) { throw; }
            catch (Exception exception)
            {
                logger.ApplicationRolesCreatingError(exception);
                throw new DataAccessException(LogMessageTexts.ErrorCreatingRoles, exception);
            }
        }
    }

}

