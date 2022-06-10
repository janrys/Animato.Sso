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
        private readonly ILogger<DeleteApplicationCommandHandler> logger;
        private const string ERROR_CREATING_APPLICATION = "Error creating application";

        public DeleteApplicationCommandHandler(IApplicationRepository applicationRepository
            , ILogger<DeleteApplicationCommandHandler> logger)
        {
            this.applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(DeleteApplicationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var application = await applicationRepository.GetById(request.ApplicationId, cancellationToken);

                if (application is not null)
                {
                    await applicationRepository.Delete(request.ApplicationId, cancellationToken);
                }

                return Unit.Value;
            }
            catch (Exceptions.ValidationException) { throw; }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_CREATING_APPLICATION);
                throw new DataAccessException(ERROR_CREATING_APPLICATION, exception);
            }
        }
    }

}
