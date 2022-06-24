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

public class CreateApplicationCommand : IRequest<Application>
{
    public CreateApplicationCommand(CreateApplicationModel application, ClaimsPrincipal user)
    {
        Application = application;
        User = user;
    }

    public CreateApplicationModel Application { get; }
    public ClaimsPrincipal User { get; }

    public class CreateApplicationCommandValidator : AbstractValidator<CreateApplicationCommand>
    {
        public CreateApplicationCommandValidator()
        {
            RuleFor(v => v.Application).NotNull().WithMessage(v => $"{nameof(v.Application)} must have a value");
            RuleFor(v => v.Application.Code).NotEmpty().WithMessage(v => $"{nameof(v.Application.Code)} must have a value");
            RuleFor(v => v.Application.Name).NotEmpty().WithMessage(v => $"{nameof(v.Application.Name)} must have a value");
            RuleFor(v => v.Application.RedirectUris).NotEmpty().WithMessage(v => $"{nameof(v.Application.RedirectUris)} must have a value");
        }
    }

    public class CreateApplicationCommandHandler : IRequestHandler<CreateApplicationCommand, Application>
    {
        private readonly OidcOptions oidcOptions;
        private readonly IApplicationRepository applicationRepository;
        private readonly ITokenFactory tokenFactory;
        private readonly ILogger<CreateApplicationCommandHandler> logger;

        public CreateApplicationCommandHandler(OidcOptions oidcOptions
            , IApplicationRepository applicationRepository
            , ITokenFactory tokenFactory
            , ILogger<CreateApplicationCommandHandler> logger)
        {
            this.oidcOptions = oidcOptions ?? throw new ArgumentNullException(nameof(oidcOptions));
            this.applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            this.tokenFactory = tokenFactory ?? throw new ArgumentNullException(nameof(tokenFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Application> Handle(CreateApplicationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var application = await applicationRepository.GetByCode(request.Application.Code, cancellationToken);

                if (application is not null)
                {
                    throw new Exceptions.ValidationException(
                        Exceptions.ValidationException.CreateFailure(nameof(request.Application.Code)
                        , $"Application with {nameof(request.Application.Code)} {request.Application.Code} already exists"));
                }

                request.Application.ValidateAndSanitize(oidcOptions, tokenFactory);
                application = new Application();
                application = request.Application.ApplyTo(application);
                return await applicationRepository.Create(application, cancellationToken);
            }
            catch (Exceptions.ValidationException) { throw; }
            catch (Exception exception)
            {
                logger.ApplicationsCreatingError(exception);
                throw new DataAccessException("Error creating application", exception);
            }
        }
    }

}

