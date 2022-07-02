namespace Animato.Sso.Application.Features.Applications;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public class DeleteApplicationCommand : IRequest<Unit>
{
    public DeleteApplicationCommand(Domain.Entities.ApplicationId applicationId, ClaimsPrincipal user)
    {
        ApplicationId = applicationId;
        User = user;
    }

    public Domain.Entities.ApplicationId ApplicationId { get; }
    public ClaimsPrincipal User { get; }

    public class DeleteApplicationCommandValidator : AbstractValidator<DeleteApplicationCommand>
    {
        public DeleteApplicationCommandValidator()
            => RuleFor(v => v.ApplicationId).NotNull().WithMessage(v => $"{nameof(v.ApplicationId)} must have a value");
    }

    public class DeleteApplicationCommandHandler : IRequestHandler<DeleteApplicationCommand, Unit>
    {
        private readonly IApplicationRepository applicationRepository;
        private readonly IApplicationRoleRepository roleRepository;
        private readonly ILogger<DeleteApplicationCommandHandler> logger;
        private const string ERROR_DELETING_APPLICATION = "Error deleting application";

        public DeleteApplicationCommandHandler(IApplicationRepository applicationRepository
            , IApplicationRoleRepository roleRepository
            , ILogger<DeleteApplicationCommandHandler> logger)
        {
            this.applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            this.roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(DeleteApplicationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var application = await applicationRepository.GetById(request.ApplicationId, cancellationToken);

                if (application is null)
                {
                    return Unit.Value;
                }

                var roles = await roleRepository.GetByApplication(request.ApplicationId, cancellationToken);
                if (roles != null && roles.Any())
                {
                    throw new Exceptions.ValidationException(
                        Exceptions.ValidationException.CreateFailure("ApplicationRole", $"Application has {roles.Count()} roles")
                        );
                }

                await applicationRepository.Delete(request.ApplicationId, cancellationToken);
                return Unit.Value;
            }
            catch (Exceptions.ValidationException) { throw; }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_DELETING_APPLICATION);
                throw new DataAccessException(ERROR_DELETING_APPLICATION, exception);
            }
        }
    }

}
