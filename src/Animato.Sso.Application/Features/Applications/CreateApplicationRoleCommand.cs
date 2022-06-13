namespace Animato.Sso.Application.Features.Applications;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Application.Features.Applications.DTOs;
using Animato.Sso.Application.Features.Users.DTOs;
using Animato.Sso.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public class CreateApplicationRoleCommand : IRequest<ApplicationRole>
{
    public CreateApplicationRoleCommand(Domain.Entities.ApplicationId applicationId, CreateApplicationRoleModel role, ClaimsPrincipal user)
    {
        ApplicationId = applicationId;
        Role = role;
        User = user;
    }

    public Domain.Entities.ApplicationId ApplicationId { get; }
    public CreateApplicationRoleModel Role { get; }
    public ClaimsPrincipal User { get; }

    public class CreateApplicationRoleCommandValidator : AbstractValidator<CreateApplicationRoleCommand>
    {
        public CreateApplicationRoleCommandValidator()
        {
            RuleFor(v => v.ApplicationId).NotNull().WithMessage(v => $"{nameof(v.ApplicationId)} must have a value");
            RuleFor(v => v.Role).NotNull().WithMessage(v => $"{nameof(v.Role)} must have a value");
        }
    }

    public class CreateApplicationRoleCommandHandler : IRequestHandler<CreateApplicationRoleCommand, ApplicationRole>
    {
        private readonly OidcOptions oidcOptions;
        private readonly IApplicationRepository applicationRepository;
        private readonly IApplicationRoleRepository roleRepository;
        private readonly ITokenFactory tokenFactory;
        private readonly ILogger<CreateApplicationRoleCommandHandler> logger;
        private const string ERROR_CREATING_APPLICATION_ROLE = "Error creating application role";

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

        public async Task<ApplicationRole> Handle(CreateApplicationRoleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var application = await applicationRepository.GetById(request.ApplicationId, cancellationToken);

                if (application is null)
                {
                    throw new NotFoundException(nameof(Application)
                        , $"Application with id {request.ApplicationId} does not exist");
                }

                request.Role.ValidateAndSanitize();
                var role = new ApplicationRole();
                role = request.Role.ApplyTo(role);
                role.ApplicationId = request.ApplicationId;
                return await roleRepository.Create(role, cancellationToken);
            }
            catch (NotFoundException) { throw; }
            catch (Exceptions.ValidationException) { throw; }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_CREATING_APPLICATION_ROLE);
                throw new DataAccessException(ERROR_CREATING_APPLICATION_ROLE, exception);
            }
        }
    }

}

