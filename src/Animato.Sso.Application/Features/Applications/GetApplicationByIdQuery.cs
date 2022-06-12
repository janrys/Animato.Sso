namespace Animato.Sso.Application.Features.Applications;
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

public class GetApplicationByIdQuery : IRequest<Application>
{
    public GetApplicationByIdQuery(Domain.Entities.ApplicationId applicationId, ClaimsPrincipal user)
    {
        ApplicationId = applicationId;
        User = user;
    }

    public Domain.Entities.ApplicationId ApplicationId { get; }
    public ClaimsPrincipal User { get; }

    public class GetApplicationByIdQueryValidator : AbstractValidator<GetApplicationByIdQuery>
    {
        public GetApplicationByIdQueryValidator()
        {
            RuleFor(v => v.ApplicationId).NotEmpty().WithMessage(v => $"{nameof(v.ApplicationId)} must have a value");
            RuleFor(v => v.User).NotNull().WithMessage(v => $"{nameof(v.User)} must have a value");
        }
    }

    public class GetApplicationByIdQueryHandler : IRequestHandler<GetApplicationByIdQuery, Application>
    {
        private readonly IApplicationRepository applicationRepository;
        private readonly ILogger<GetApplicationByIdQueryHandler> logger;
        private const string ERROR_LOADING_APPLICATIONS = "Error loading applications";

        public GetApplicationByIdQueryHandler(IApplicationRepository applicationRepository, ILogger<GetApplicationByIdQueryHandler> logger)
        {
            this.applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Application> Handle(GetApplicationByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return await applicationRepository.GetById(request.ApplicationId, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_LOADING_APPLICATIONS);
                throw new DataAccessException(ERROR_LOADING_APPLICATIONS, exception);
            }
        }
    }

}
