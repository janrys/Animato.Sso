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

public class UpdateApplicationCommand : IRequest<Application>
{
    public UpdateApplicationCommand(Domain.Entities.ApplicationId applicationId
        , CreateApplicationModel application
        , ClaimsPrincipal user)
    {
        ApplicationId = applicationId;
        Application = application;
        User = user;
    }

    public Domain.Entities.ApplicationId ApplicationId { get; }
    public CreateApplicationModel Application { get; }
    public ClaimsPrincipal User { get; }

    public class UpdateApplicationCommandValidator : AbstractValidator<UpdateApplicationCommand>
    {
        public UpdateApplicationCommandValidator()
        {
            RuleFor(v => v.Application).NotNull().WithMessage(v => $"{nameof(v.Application)} must have a value");
            RuleFor(v => v.Application.Code).NotEmpty().WithMessage(v => $"{nameof(v.Application.Code)} must have a value");
            RuleFor(v => v.Application.Name).NotEmpty().WithMessage(v => $"{nameof(v.Application.Name)} must have a value");
            RuleFor(v => v.Application.RedirectUris).NotEmpty().WithMessage(v => $"{nameof(v.Application.RedirectUris)} must have a value");
            RuleFor(v => v.ApplicationId).NotEmpty().WithMessage(v => $"{nameof(v.ApplicationId)} must have a value");
        }
    }

    public class UpdateApplicationCommandHandler : IRequestHandler<UpdateApplicationCommand, Application>
    {
        private readonly OidcOptions oidcOptions;
        private readonly IApplicationRepository applicationRepository;
        private readonly ITokenFactory tokenFactory;
        private readonly ILogger<UpdateApplicationCommandHandler> logger;
        private const string ERROR_UPDATING_APPLICATION = "Error updating application";

        public UpdateApplicationCommandHandler(OidcOptions oidcOptions
            , IApplicationRepository applicationRepository
            , ITokenFactory tokenFactory
            , ILogger<UpdateApplicationCommandHandler> logger)
        {
            this.oidcOptions = oidcOptions ?? throw new ArgumentNullException(nameof(oidcOptions));
            this.applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            this.tokenFactory = tokenFactory ?? throw new ArgumentNullException(nameof(tokenFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Application> Handle(UpdateApplicationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var application = await applicationRepository.GetById(request.ApplicationId, cancellationToken);

                if (application is null)
                {
                    throw new NotFoundException(nameof(Domain.Entities.Application), request.ApplicationId);
                }

                request.Application.ValidateAndSanitize(oidcOptions, tokenFactory);
                application = request.Application.ApplyTo(application);

                return await applicationRepository.Update(application, cancellationToken);
            }
            catch (NotFoundException) { throw; }
            catch (Exceptions.ValidationException) { throw; }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_UPDATING_APPLICATION);
                throw new DataAccessException(ERROR_UPDATING_APPLICATION, exception);
            }
        }
    }

}
