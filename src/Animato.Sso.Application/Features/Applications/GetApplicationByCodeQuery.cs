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

public class GetApplicationByCodeQuery : IRequest<Application>
{
    public GetApplicationByCodeQuery(string code, ClaimsPrincipal user)
    {
        Code = code;
        User = user;
    }

    public string Code { get; }
    public ClaimsPrincipal User { get; }

    public class GetApplicationByCodeQueryValidator : AbstractValidator<GetApplicationByCodeQuery>
    {
        public GetApplicationByCodeQueryValidator()
        {
            RuleFor(v => v.Code).NotEmpty().WithMessage(v => $"{nameof(v.Code)} must have a value");
            RuleFor(v => v.User).NotNull().WithMessage(v => $"{nameof(v.User)} must have a value");
        }
    }

    public class GetApplicationByCodeQueryHandler : IRequestHandler<GetApplicationByCodeQuery, Application>
    {
        private readonly IApplicationRepository applicationRepository;
        private readonly ILogger<GetApplicationByCodeQueryHandler> logger;
        private const string ERROR_LOADING_APPLICATIONS = "Error loading applications";

        public GetApplicationByCodeQueryHandler(IApplicationRepository applicationRepository, ILogger<GetApplicationByCodeQueryHandler> logger)
        {
            this.applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Application> Handle(GetApplicationByCodeQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return await applicationRepository.GetByCode(request.Code, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_LOADING_APPLICATIONS);
                throw new DataAccessException(ERROR_LOADING_APPLICATIONS, exception);
            }
        }
    }

}
